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

namespace OgameBot.Engine.Tasks
{
    public class FarmingBot
    {
        private readonly OGameClient _client;
        private OGameRequestBuilder RequestBuilder => _client.RequestBuilder;

        private Coordinate _from, _to;
        private ScannerJob _scanner;
        private int _range;
        private int _minRanking;
        private string _player;
        private string _planet;
        private Random _sleepTime = new Random();

        public FarmingBot(OGameClient client, string player, string planet, int range, int minRanking)
        {
            _client = client;
            _player = player;
            _planet = planet;
            _range = range;
            _minRanking = minRanking;
        }

        public void Start()
        {
            Planet source = null;
            using (BotDb db = new BotDb())
            {
                source = db.Planets.Where(s => s.Name == _planet && s.Player.Name ==_player).FirstOrDefault();
            }

            var req = _client.RequestBuilder.GetPage(PageType.Galaxy, source.PlanetId);
            _client.IssueRequest(req);

            if (source == null) throw new ArgumentException("Planet not found");

            // Start scanner
            _from = source.Coordinate;
            _from.System = (short)Math.Max(_from.System - _range, 1);
            _from.Planet = 1;

            _to = source.Coordinate;
            _to.System = (short)Math.Min(_to.System + _range, 499);
            _to.Planet = 15;

            _scanner = new ScannerJob(_client, _from, _to);
            _scanner.OnJobFinished += () => Task.Factory.StartNew(Worker);
            _scanner.Start();

            // Start worker
            
        }

        private void Worker()
        {
            _scanner.Stop();
            IEnumerable<Planet> farms = GetFarms();
            Logger.Instance.Log(LogLevel.Info, $"Got {farms.Count()} farms, probing...");

            SendProbes(farms);
            Logger.Instance.Log(LogLevel.Info, "Sending probes finished, waiting to come back and read messages");
            WaitForProbes();
            
            // Read messages
            ReadAllMessagesCommand cmd = new ReadAllMessagesCommand(_client);
            cmd.Run();
            var messages = cmd.ParsedObjects.OfType<EspionageReport>();

            Resources totalPlunder = Attack(messages);
            Logger.Instance.Log(LogLevel.Info, $"Job done, theoretical total plunder: {totalPlunder}");
        }
              

        private IEnumerable<Planet> GetFarms()
        {
            using (BotDb db = new BotDb())
            {
                var farms = db.Planets.Where(s =>
                        s.LocationId >= _from.Id && s.LocationId <= _to.Id
                     && (s.Player.Status.HasFlag(PlayerStatus.Inactive) || s.Player.Status.HasFlag(PlayerStatus.LongInactive))
                     && !s.Player.Status.HasFlag(PlayerStatus.Vacation)
                     && !s.Player.Status.HasFlag(PlayerStatus.Admin)
                     && s.Player.Ranking < _minRanking
                ).ToList();

                return farms;
            }
        }

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

        private void SendProbes(IEnumerable<Planet> farms)
        {
            HttpRequestMessage req = RequestBuilder.GetPage(PageType.Galaxy);
            ResponseContainer resp = _client.IssueRequest(req);

            var info = resp.GetParsedSingle<OgamePageInfo>();
            if (info.PlanetName != _planet)
            {
                throw new ApplicationException("Not where we should be");
            }

            string token = info.MiniFleetToken;
            bool wasSuccessful = false;
            int retry = 0;

            foreach (Planet farm in farms)
            {
                wasSuccessful = false;
                retry = 0;

                while (!wasSuccessful && retry < 5)
                {
                    req = RequestBuilder.GetMiniFleetSendMessage(MissionType.Espionage, farm.Coordinate, 2, token);
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
            ResponseContainer resp = _client.IssueRequest(RequestBuilder.GetPage(PageType.Fleet));
            FleetSlotCount slotCount = resp.GetParsedSingle<FleetSlotCount>();


            Logger.Instance.Log(LogLevel.Info, $"{messages.Count()} Messages parsed, sending ships. Currently {slotCount.Current} fleets out of {slotCount.Max} available slots.");

            //Check if the planet wasn't changed in the meantime (ie. by user action), we'd be sending cargos for a long trip
            OgamePageInfo info = resp.GetParsedSingle<OgamePageInfo>();
            if (info.PlanetName != _planet)
            {
                throw new ApplicationException("Not where we should be when sending fleets");
            }

            // Check how many cargos are there
            int? cargoCount = resp.GetParsed<DetectedShip>().Where(s => s.Ship == ShipType.LargeCargo).FirstOrDefault()?.Count;
            if (!cargoCount.HasValue || cargoCount == 0)
            {
                Logger.Instance.Log(LogLevel.Error, "There are no cargos on the planet");
                return new Resources();
            }

            int slotsAvailable = slotCount.Max - slotCount.Current - 1;
            if (slotsAvailable <= 0)
            {
                Logger.Instance.Log(LogLevel.Error, "No slots available");
                return new Resources();
            }

            // Get all the messages where defense and ships sections were found, but were empty
            // We want that list in a descending order of total resources available for us to plunder
            var farmsToAttack = messages.Where(m =>
                                               m.Details.HasFlag(ReportDetails.Defense) && m.DetectedDefence == null &&
                                               m.Details.HasFlag(ReportDetails.Ships) && m.DetectedShips == null)
                                        .OrderByDescending(m => m.Resources.Total);

            Resources totalPlunder = new Resources();
            foreach (var farm in farmsToAttack)
            {
                
                var fleetComposition = FleetComposition.ToPlunder(farm.Resources, ShipType.LargeCargo);
                int cargosToUse = fleetComposition.Ships[ShipType.LargeCargo];
                // stop farming when:
                //  - just sending 1 cargo (not worth it)
                //  - there are no remaining cargos on planet
                //  - no slots available
                if (cargosToUse == 1 || cargoCount <= 0 || slotsAvailable == 0)
                {
                    break;
                }

                Thread.Sleep(3000 + _sleepTime.Next(2000));
                Resources plunder = farm.Resources / 2;
                plunder.Energy = 0;
                totalPlunder = totalPlunder + plunder;
                Logger.Instance.Log(LogLevel.Info, $"Sending {cargosToUse} to planet {farm.Coordinate} to plunder {plunder}");

                SendFleetCommand attack = new SendFleetCommand(_client)
                {
                    Mission = MissionType.Attack,
                    Destination = farm.Coordinate,
                    Source = info.PlanetCoord,
                    Fleet = fleetComposition
                };
                attack.Run();

                cargoCount -= cargosToUse;
                slotsAvailable--;

            }

            return totalPlunder;
        }
    }
}
 