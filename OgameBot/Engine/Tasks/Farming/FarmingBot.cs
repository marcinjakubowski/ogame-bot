using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using OgameBot.Db;
using System;
using OgameBot.Objects;
using OgameBot.Engine.Commands;
using System.Collections.Generic;
using OgameBot.Logging;
using OgameBot.Engine.Parsing.Objects;
using OgameBot.Objects.Types;
using System.Net.Http;
using ScraperClientLib.Engine;
using Newtonsoft.Json.Linq;
using OgameBot.Engine.Tasks.Farming.Strategies;

namespace OgameBot.Engine.Tasks.Farming
{
    public class FarmingBot
    {
        private readonly OGameClient _client;
        private OGameRequestBuilder RequestBuilder => _client.RequestBuilder;

        private SystemCoordinate _from, _to;
        private ScannerJob _scanner;
        private int _range;
        private int _planet;
        private Random _sleepTime = new Random();
        private IFarmingStrategy _strategy;

        public int ProbeCount { get; set; } = 3;

        public FarmingBot(OGameClient client, int planet, int range, IFarmingStrategy strategy)
        {
            _client = client;
            _planet = planet;
            _range = range;
            _strategy = strategy;
        }

        public void Start()
        {
            var req = _client.RequestBuilder.GetPage(PageType.Galaxy, _planet == 0 ? null : (int?)_planet);
            var resp = _client.IssueRequest(req);

            var source = resp.GetParsedSingle<OgamePageInfo>();
            _planet = source.PlanetId;

            // Start scanner
            _from = source.PlanetCoord;
            _from.System = (short)Math.Max(_from.System - _range, 1);

            _to = source.PlanetCoord;
            _to.System = (short)Math.Min(_to.System + _range, 499);

            _scanner = new ScannerJob(_client, _from, _to);
            _scanner.OnJobFinished += () => Task.Factory.StartNew(Worker);
            _scanner.Start();

            // Start worker
            
        }

        private void Worker()
        {
            _scanner.Stop();
            IEnumerable<Planet> farms = _strategy.GetFarms(_from, _to);
            Logger.Instance.Log(LogLevel.Info, $"Got {farms.Count()} farms, probing...");

            SendProbes(farms);
            Logger.Instance.Log(LogLevel.Info, "Sending probes finished, waiting to come back and read messages");
            WaitForProbes();
            
            // Read messages
            ReadAllMessagesCommand cmd = new ReadAllMessagesCommand(_client);
            cmd.Run();
            var messages = cmd.ParsedObjects.OfType<EspionageReport>();

            Logger.Instance.Log(LogLevel.Info, $"{messages.Count()} Messages parsed, sending ships.");
            _strategy.OnBeforeAttack();
            Resources totalPlunder = Attack(messages);
            _strategy.OnAfterAttack();
            Logger.Instance.Log(LogLevel.Info, $"Job done, theoretical total plunder: {totalPlunder}");
        }
              
        /* Extract to IFarmingStrategy
        private IEnumerable<Planet> GetFarmsToScanForFleet()
        {
            using (BotDb db = new BotDb())
            {
                var farms = db.Planets.Where(s =>
                                        s.LocationId >= _from.Id && s.LocationId <= _to.Id
                                     && !((s.Player.Status.HasFlag(PlayerStatus.Inactive) || s.Player.Status.HasFlag(PlayerStatus.LongInactive)))
                                     && !s.Player.Status.HasFlag(PlayerStatus.Vacation)
                                     && !s.Player.Status.HasFlag(PlayerStatus.Admin)
                                     && !s.Player.Status.HasFlag(PlayerStatus.Noob)
                                     && (!s.Player.Status.HasFlag(PlayerStatus.Strong) || s.Player.Status.HasFlag(PlayerStatus.Outlaw))
                                ).ToList();

                return farms;
            }
        }
        */

        private void SendProbes(IEnumerable<Planet> farms)
        {
            HttpRequestMessage req = RequestBuilder.GetPage(PageType.Galaxy, _planet);
            ResponseContainer resp = _client.IssueRequest(req);
            var info = resp.GetParsedSingle<OgamePageInfo>();

            string token = info.MiniFleetToken;
            bool wasSuccessful = false;
            int retry = 0;

            int count = farms.Count();

            foreach (Planet farm in farms)
            {
                
                wasSuccessful = false;
                retry = 0;

                while (!wasSuccessful && retry < 5)
                {
                    req = RequestBuilder.GetMiniFleetSendMessage(MissionType.Espionage, farm.Coordinate, ProbeCount, token);
                    resp = _client.IssueRequest(req);

                    //#todo parse to class instead of jobject
                    //{  
                    //"response":{  
                    //"message":"Send espionage probe to:",
                    //"type":1,
                    //"slots":8,
                    //"probes":18,
                    //"recyclers":0,
                    //"missiles":0,
                    //"shipsSent":2,
                    //"coordinates":{  
                    //"galaxy":3,
                    //"system":55,
                    //"position":8
                    //},
                    //"planetType":1,
                    //"success":true
                    //},
                    //"newToken":"fb74477ff26fd4ba9c41c110aa295baf"
                    //}

                    JObject result = JObject.Parse(resp.Raw.Value);
                    wasSuccessful = (bool)result["response"]["success"];
                    if (!wasSuccessful)
                    {
                        retry++;
                        Thread.Sleep(2000 + _sleepTime.Next(4000));
                    }
                    token = result["newToken"].ToString();

                    Thread.Sleep(1000 + _sleepTime.Next(1000));

                    if (--count % 10 == 0 && count > 0)
                    {
                        Logger.Instance.Log(LogLevel.Info, $"{count} remaining to scan...");
                    }
                }

                if (!wasSuccessful)
                {
                    Logger.Instance.Log(LogLevel.Error, $"Sending probes to {farm.Coordinate} failed.");
                }
            }
        }

        private void WaitForProbes()
        {
            Thread.Sleep(5000 + _sleepTime.Next(2000));

            IEnumerable<FleetInfo> probes;
            do
            {
                HttpRequestMessage req = RequestBuilder.GetPage(PageType.FleetMovement);
                ResponseContainer resp = _client.IssueRequest(req);

                probes = resp.GetParsed<FleetInfo>().Where(fi => fi.MissionType == MissionType.Espionage);
                if (probes.Any())
                {
                    Thread.Sleep(3000 + _sleepTime.Next(2000));
                }
            } while (probes.Any());
        }

        private Resources Attack(IEnumerable<EspionageReport> messages)
        {
            //Check if the planet wasn't changed in the meantime (ie. by user action), we'd be sending cargos for a long trip
            ResponseContainer resp = _client.IssueRequest(RequestBuilder.GetPage(PageType.Fleet, _planet));
            OgamePageInfo info = resp.GetParsedSingle<OgamePageInfo>();

            Resources totalPlunder = new Resources();
            foreach (var farm in _strategy.GetTargets(messages))
            {
                Thread.Sleep(3000 + _sleepTime.Next(2000));
                totalPlunder += farm.ExpectedPlunder;
                Logger.Instance.Log(LogLevel.Info, $"Attacking planet {farm.Destination} to plunder {farm.ExpectedPlunder}");

                SendFleetCommand attack = new SendFleetCommand(_client)
                {
                    Mission = MissionType.Attack,
                    Destination = farm.Destination,
                    Source = info.PlanetCoord,
                    Fleet = farm.Fleet
                };
                attack.Run();
            }

            return totalPlunder;
        }
    }
}
 