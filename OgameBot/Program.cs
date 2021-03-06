﻿using System;
using System.Globalization;
using System.IO;
using Newtonsoft.Json;
using OgameBot.Engine;
using OgameBot.Engine.Savers;
using OgameBot.Engine.Commands;
using OgameBot.Logging;
using OgameBot.Objects;
using ScraperClientLib.Engine.Interventions;
using ScraperClientLib.Engine.Parsing;
using OgameBot.Engine.Commands.Farming;
using OgameBot.Engine.Commands.Farming.Strategies;
using OgameBot.Engine.Injects;
using OgameBot.Proxy;
using System.Collections.Specialized;
using System.Threading;
using OgameBot.Db;
using OgameBot.Engine.Tasks;
using OgameBot.Objects.Types;
using System.Linq;
using System.Security;
using OgameBot.Engine.Parsing;
using OgameBot.Utilities;

namespace OgameBot
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Fuck that, it's impossible to make Mono accept certificates from OGame, at least I'm too stupid to do it apparently.
            if (IsRunningOnMono())
            {
                System.Net.ServicePointManager.ServerCertificateValidationCallback += delegate { return true; };
            }

            if (!File.Exists("config.json"))
            {
                Console.WriteLine("Please copy config.template.json to config.json and fill it out");
                return;
            }

            IExitSignal onExitSignal = null;

            if (IsRunningOnMono())
            {
                onExitSignal = new UnixExitSignal();
            }
            else
            {
                onExitSignal = new WindowsExitSignal();
            }

            Config config = JsonConvert.DeserializeObject<Config>(File.ReadAllText("config.json"));
            Logger.Instance.MinimumLogLevel = config.LogLevel;
            Logger.Instance.IncludeTimestamp = config.LogIncludeTimestamp;
            Logger.Instance.Log(LogLevel.Info, $"Loaded settings, user: {config.Username}, server: {config.Server}");

            // Setup
            OGameStringProvider stringProvider = OGameStringProvider.Load(@"Resources/strings-en.json");
            CultureInfo clientServerCulture = CultureInfo.GetCultureInfo("da-DK");
            var commander = new CommandBase.Commander();

            // Processing
            OGameClient client = new OGameClient(config.Server, stringProvider, config.Username, config.Password, commander);
            client.Settings.ServerUtcOffset = TimeSpan.FromHours(1);
            client.Settings.Galaxies = 8;
            client.Settings.Systems = 499;
            client.ServerCulture = clientServerCulture;

            Logger.Instance.Log(LogLevel.Debug, "Prepared OGameClient");

            // Savers
            client.RegisterSaver(new GalaxyPageSaver());
            client.RegisterSaver(new EspionageReportSaver());
            client.RegisterSaver(new GalaxyPageDebrisSaver());
            client.RegisterSaver(new MessageSaver());
            client.RegisterSaver(new PlayerPlanetSaver());
            client.RegisterSaver(new GalaxyActivitySaver());
            client.RegisterSaver(new HostileAttackEmailSender(config.HostileWarning.From, config.HostileWarning.To, config.HostileWarning.Server, config.HostileWarning.Login, config.HostileWarning.Password));

            // Injects
            client.RegisterInject(new CommandsInject());
            client.RegisterInject(new CargosForTransportInject());
            client.RegisterInject(new PlanetExclusiveInject(client));
            client.RegisterInject(new CommonInject());
            client.RegisterInject(new BuildQueueInject());
            client.RegisterInject(new CustomPlanetOrderInject(config.CustomOrder));
            client.RegisterInject(new EventListTotalsInject());
            
            // UA stuff
            client.RegisterDefaultHeader("Accept-Language", "en-GB,en;q=0.8,da;q=0.6");
            client.RegisterDefaultHeader("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
            client.RegisterDefaultHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/52.0.2743.116 Safari/537.36");

            Logger.Instance.Log(LogLevel.Debug, "Prepared user agent");

            // Show details
            foreach (IInterventionHandler item in client.GetIntervention())
                Logger.Instance.Log(LogLevel.Info, $"Loaded Intervention Handler: {item.GetType().FullName}");

            foreach (BaseParser item in client.GetParsers())
                Logger.Instance.Log(LogLevel.Info, $"Loaded Parser: {item.GetType().FullName}");

            foreach (SaverBase item in client.GetSavers())
                Logger.Instance.Log(LogLevel.Info, $"Loaded Saver: {item.GetType().FullName}");

            // Start proxy
            OgameClientProxy proxy = new OgameClientProxy(config.ListenAddress, config.ListenPort, client);
            proxy.SubstituteRoot = new Uri($"https://{config.Server}");
            proxy.Start();

            Logger.Instance.Log(LogLevel.Success, $"Prepared reverse proxy, visit: {proxy.ListenPrefix}");

            // Kick-off
            client.IssueRequest(client.RequestBuilder.GetPage(Objects.Types.PageType.Overview));

            // Example job
            ApiImporterJob job1 = new ApiImporterJob(client, new DirectoryInfo("temp"));
            job1.Start();

            AuctionMonitor monitor = new AuctionMonitor(client);
            client.RegisterInject(new AuctionStatusInject(monitor));
            monitor.Start();


            SessionKeepAliveJob job3 = new SessionKeepAliveJob(client, config.SessionKeepaliveMode);
            if (config.SessionKeepaliveMode == SessionKeepAliveMode.Single)
            {
                job3.PlanetId = config.SessionKeepalivePlanet;
            }
            job3.Start();

            commander.Start();
            
            Action<int> recallAction = (fleet) =>
            {
                RecallFleetCommand recall = new RecallFleetCommand()
                {
                    FleetId = fleet
                };
                recall.Run();
            };
                       

            if (config.FleetToRecall > 0)
            {
                recallAction(config.FleetToRecall);
                Thread.Sleep(5000);
                return;
            }

            if (config.SystemsToScan?.Count > 0)
            {
                SystemScanner sysScanner = new SystemScanner(config.SystemsToScan.Select(z => SystemCoordinate.Parse(z)));
                sysScanner.Start();
            }
            SetupProxyCommands(client, config, proxy);

            onExitSignal.Exit += (sender, eventArgs) =>
            {
                client.SaveCookies();
            };

            Console.TreatControlCAsInput = true;
            Console.ReadKey(true);
            client.SaveCookies();
        }

        public static bool IsRunningOnMono()
        {
            return Type.GetType("Mono.Runtime") != null;
        }

        private static void SetupProxyCommands(OGameClient client, Config config, OgameClientProxy proxy)
        {
            proxy.AddCommand("bid", (parameters) =>
            {
                ResourceType resourceType = ResourceType.Deuterium;
                string resource = parameters["resource"];
                if (resource != null)
                {
                    if (resource.Equals("m", StringComparison.InvariantCultureIgnoreCase) || resource.Equals("metal", StringComparison.InvariantCultureIgnoreCase))
                        resourceType = ResourceType.Metal;
                    else if (resource.Equals("c", StringComparison.InvariantCultureIgnoreCase) || resource.Equals("crystal", StringComparison.InvariantCultureIgnoreCase))
                        resourceType = ResourceType.Crystal;
                    else if (resource.Equals("d", StringComparison.InvariantCultureIgnoreCase) || resource.Equals("deuterium", StringComparison.InvariantCultureIgnoreCase))
                        resourceType = ResourceType.Deuterium;
                }

                BidAuctionCommand bid = new BidAuctionCommand()
                {
                    PlanetId = int.Parse(parameters["cp"]),
                    BidResource = resourceType
                };
                bid.Run();
            });

            proxy.AddCommand("transport", (parameters) =>
            {
                TransportAllCommand transportAll = new TransportAllCommand()
                {
                    PlanetId = int.Parse(parameters["from"]),
                    Destination = DbHelper.GetPlanetCoordinateByCp(int.Parse(parameters["to"]))
                };
                transportAll.Run();
            });

            proxy.AddCommand("readmessages", (parameters) =>
            {
                (new ReadAllMessagesCommand()).Run();
            });

            proxy.AddCommand("scanaround", (parameters) =>
            {
                int range = 60;

                if (parameters["range"] != null) range = int.Parse(parameters["range"]);

                new ScanAroundOwnCommand()
                {
                    Range = range
                }.Run();
            });


            proxy.AddCommand("hunt", (parameters) =>
            {
                IFarmingStrategy strategy = new FleetFinderStrategy()
                {
                    MaxRanking = config.Farming.HuntMaximumRanking > 0 ? config.Farming.HuntMaximumRanking : 400,
                    MinRanking = config.Farming.HuntMinimumRanking > 0 ? config.Farming.HuntMinimumRanking : 600,
                    MoonsOnly = config.Farming.HuntMoonsOnly,
                    ProbeCount = config.Farming.HuntProbeCount > 0 ? config.Farming.HuntProbeCount : 4
                };
                Farm(client, config, strategy, parameters).Run();
            });

            proxy.AddCommand("build", (parameters) =>
            {
                BuildCommand cmd = new BuildCommand()
                {
                    PlanetId = int.Parse(parameters["cp"]),
                    BuildingToBuild = (BuildingType)int.Parse(parameters["id"])
                };
                cmd.Run();
            });

            proxy.AddCommand("fs", (parameters) =>
            {
                FleetSaveCommand cmd = new FleetSaveCommand()
                {
                    PlanetId = int.Parse(parameters["cp"]),
                    ReturnTime = DateTimeOffset.Now.AddMinutes(int.Parse(parameters["in"]))
                };
                cmd.Run();
            });

            proxy.AddCommand("farm", (parameters) =>
            {
                IFarmingStrategy strategy = new InactiveFarmingStrategy()
                {
                    MinimumCargosToSend = 2,
                    SlotsLeaveRemaining = parameters["slots"] == null ? 1 : int.Parse(parameters["slots"]),
                    MinimumTotalStorageLevel = 5,
                    ResourcePriority = new Resources(1, 2, 1),
                    MinimumRanking = config.Farming.InactiveMinimumRanking
                };
                Farm(client, config, strategy, parameters).Run();
            });

            proxy.AddCommand("schedule", (parameters) =>
            {
                long unixTime = 0;


                var runAt = parameters["at"];
                var runIn = parameters["in"];

                if (runAt != null) unixTime = long.Parse(runAt);
                else if (runIn != null)
                {
                    int secs = 0;

                    if (runIn.Contains('h') || runIn.Contains('m'))
                    {
                        // this should be a function, or maybe there's a way to use TimeSpan.ParseExact to parse this correctly
                        int hours = 0, minutes = 0;

                        int hIx = runIn.IndexOf('h');
                        int mIx = runIn.IndexOf('m');

                        if (hIx != -1)
                        {
                            hours = int.Parse(runIn.Substring(0, hIx));
                        }
                        if (mIx != -1)
                        {
                            hIx++;
                            minutes = int.Parse(runIn.Substring(hIx, mIx - hIx));
                        }

                        secs = hours * 3600 + minutes * 60;
                    }
                    else
                    {
                        secs = int.Parse(runIn);
                    }

                    unixTime = DateTimeOffset.Now.AddSeconds(secs).ToUnixTimeSeconds();
                }
                string cmd = parameters["cmd"];

                parameters.Remove("cmd");
                parameters.Remove("at");
                parameters.Remove("in");

                var command = new RunProxyCommand()
                {
                    Command = cmd,
                    Parameters = parameters
                };

                client.Commander.Run(command, DateTimeOffset.FromUnixTimeSeconds(unixTime));
            });

            proxy.AddCommand("fake", (parameters) =>
            {
                FakePlanetExclusive op = new FakePlanetExclusive()
                {
                    PlanetId = int.Parse(parameters["cp"])
                };
                op.Run();
            });

            proxy.AddCommand("fullscan", (parameters) => new ScanCommand()
            {
                From = new SystemCoordinate(1, 1),
                To = new SystemCoordinate(6, 499),
            }.Run());

            proxy.AddCommand("expedition", (parameters) =>
            {
                int cp = int.Parse(parameters["cp"]);
                Coordinate dest;
                using (BotDb db = new BotDb())
                {
                    var here = db.Planets.FirstOrDefault(p => p.PlanetId == cp);

                    dest = Coordinate.Create(here.Coordinate, 16, CoordinateType.Unknown);
                }

                SendFleetCommand cmd = new SendFleetCommand()
                {
                    PlanetId = cp,
                    Destination = dest,
                    Mission = MissionType.Expedition
                };
                cmd.Fleet = new FleetComposition();
                cmd.Fleet.Ships[ShipType.Bomber] = 1;
                cmd.Fleet.Ships[ShipType.LightFighter] = 5;
                cmd.Fleet.Ships[ShipType.LargeCargo] = 192;
                cmd.Fleet.Ships[ShipType.EspionageProbe] = 1;

                cmd.Run();
            });

            proxy.AddCommand("jump", (parameters) =>
            {
                int cp = int.Parse(parameters["from"]);
                int dest = int.Parse(parameters["to"]); ;

                GateJumpCommand cmd = new GateJumpCommand()
                {
                    PlanetId = cp,
                    Destination = dest,
                    Fleet = null
                };
                cmd.Run();
            }) ;
            
        }

        private static FarmCommand Farm(OGameClient client, Config config, IFarmingStrategy strategy, NameValueCollection parameters)
        {
            int range;
            if (!int.TryParse(parameters["range"], out range))
                range = config.Farming.DefaultRange;

            int planetId = 0;
            if (parameters["cp"] != null)
            {
                if (!int.TryParse(parameters["cp"], out planetId))
                {
                    planetId = 0;
                }
            }

            FarmCommand bot = new FarmCommand()
            {
                PlanetId = planetId,
                Range = range,
                Strategy = strategy,
            };
            return bot;
        }
    }
}
