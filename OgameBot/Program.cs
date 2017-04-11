using System;
using System.Globalization;
using System.IO;
using Newtonsoft.Json;
using OgameBot.Engine;
using OgameBot.Engine.Savers;
using OgameBot.Engine.Tasks;
using OgameBot.Logging;
using OgameBot.Objects;
using ScraperClientLib.Engine.Interventions;
using ScraperClientLib.Engine.Parsing;
using OgameBot.Engine.Tasks.Farming;
using OgameBot.Engine.Tasks.Farming.Strategies;
using OgameBot.Engine.Injects;
using OgameBot.Proxy;
using OgameBot.Engine.Commands;
using System.Collections.Specialized;
using System.Threading;

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

            Config config = JsonConvert.DeserializeObject<Config>(File.ReadAllText("config.json"));
            Logger.Instance.MinimumLogLevel = config.LogLevel;
            Logger.Instance.Log(LogLevel.Info, $"Loaded settings, user: {config.Username}, server: {config.Server}");

            // Setup
            OGameStringProvider stringProvider = OGameStringProvider.Load(@"Resources/strings-en.json");
            CultureInfo clientServerCulture = CultureInfo.GetCultureInfo("da-DK");

            // Processing
            OGameClient client = new OGameClient(config.Server, stringProvider, config.Username, config.Password);
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

            // Injects
            client.RegisterInject(new CommandsInject());
            client.RegisterInject(new CargosForTransportInject());
            client.RegisterInject(new PlanetExclusiveInject(client));
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

            /*MessageReaderJob job2 = new MessageReaderJob(client);
            job2.Start();*/

            SessionKeepAliveJob job3 = new SessionKeepAliveJob(client);
            job3.Start();

            Builder buildJob = new Builder(client);
            buildJob.Start();

            //ScannerJob s = new ScannerJob(client, new SystemCoordinate(1, 1), new SystemCoordinate(6, 499));
            //s.Start();

            Action<int> recallAction = (fleet) =>
            {
                RecallFleetCommand recall = new RecallFleetCommand(client, fleet);
                recall.Run();
            };

            proxy.AddCommand("transport", (parameters) =>
            {
                TransportAllCommand transportAll = new TransportAllCommand(client, int.Parse(parameters["from"]), int.Parse(parameters["to"]));
                transportAll.Run();
            });
            
            proxy.AddCommand("hunt", (parameters) =>
            {
                IFarmingStrategy strategy = new FleetFinderStrategy(client)
                {
                    MaxRanking = config.HuntMaximumRanking > 0 ? config.HuntMaximumRanking : 400
                };
                Farm(client, config, strategy, parameters);
            });

            proxy.AddCommand("farm", (parameters) =>
            {
                IFarmingStrategy strategy = new InactiveFarmingStrategy(client)
                {
                    MinimumCargosToSend = 2,
                    SlotsLeaveRemaining = parameters["slots"] == null ? 1 : int.Parse(parameters["slots"]),
                    MinimumTotalStorageLevel = 5,
                    ResourcePriority = new Resources(1, 2, 1),
                    MinimumRanking = config.FarmMinimumRanking
                };
                Farm(client, config, strategy, parameters);
            });

            proxy.AddCommand("recall", (parameters) =>
            {
                recallAction(int.Parse(parameters["fleet"]));
            });

            proxy.AddCommand("fake", (parameters) =>
            {
                FakePlanetExclusive op = new FakePlanetExclusive(client);
                op.Run();
            });

            if (config.FleetToRecall > 0)
            {
                recallAction(config.FleetToRecall);
                Thread.Sleep(5000);
                return;
            }

            // Work
            Console.ReadLine();
        }

        public static bool IsRunningOnMono()
        {
            return Type.GetType("Mono.Runtime") != null;
        }

        private static void Farm(OGameClient client, Config config, IFarmingStrategy strategy, NameValueCollection parameters)
        {
            int range;
            if (!int.TryParse(parameters["range"], out range))
                range = config.FarmRange;

            int planetId = 0;
            if (parameters["cp"] != null)
            {
                if (!int.TryParse(parameters["cp"], out planetId))
                {
                    planetId = 0;
                }
            }

            int delay = 0;

            if (parameters["delay"] != null)
            {
                if (!int.TryParse(parameters["delay"], out delay))
                {
                    delay = 0;
                }
            }

            FarmingBot bot = new FarmingBot(client, planetId, range, strategy)
            {
                Delay = delay
            };
        }
    }
}
