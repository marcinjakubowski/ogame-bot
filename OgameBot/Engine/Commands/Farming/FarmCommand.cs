using System.Threading;
using System.Linq;
using OgameBot.Db;
using System;
using OgameBot.Objects;
using System.Collections.Generic;
using OgameBot.Logging;
using OgameBot.Engine.Parsing.Objects;
using OgameBot.Objects.Types;
using System.Net.Http;
using ScraperClientLib.Engine;
using OgameBot.Engine.Commands.Farming.Strategies;

namespace OgameBot.Engine.Commands.Farming
{
    public class FarmCommand : CommandBase, IPlanetExclusiveOperation
    {
        private OGameRequestBuilder RequestBuilder => Client.RequestBuilder;

        public string Name
        {
            get
            {
                using (BotDb db = new BotDb())
                {
                    Planet planet = db.Planets.Where(p => p.PlanetId == PlanetId).First();
                    return $"Farming using {Strategy} in range {Range}";
                }
            }
        }
        public string Progress { get; private set; }
        public int Range { get; set; }
        public IFarmingStrategy Strategy { get; set; }

        private SystemCoordinate _from, _to;
        private Random _sleepTime = new Random();
        private string _token;

        public FarmCommand()
        {
            Client.OnResponseReceived += PageChanged;
        }

        private void PageChanged(ResponseContainer resp)
        {
            string minifleetToken = resp.GetParsedSingle<OgamePageInfo>(false)?.MiniFleetToken;
            if (minifleetToken != null)
            {
                _token = minifleetToken;
            }
        }

        private CommandQueueElement Farm()
        {
            using (Client.EnterPlanetExclusive(this))
            {
                IEnumerable<Planet> farms = Strategy.GetFarms(_from, _to);
                Logger.Instance.Log(LogLevel.Info, $"Got {farms.Count()} farms, probing...");

                SendProbes(farms);
                Logger.Instance.Log(LogLevel.Info, "Sending probes finished, waiting to come back and read messages");
                WaitForProbes();

                // Read messages
                ReadAllMessagesCommand cmd = new ReadAllMessagesCommand();
                cmd.Run();

                // Get the newly read messages, but limit to planets in range, in case there was something else going on in the meantime
                // or unread messages from earlier
                var messages = cmd.ParsedObjects.OfType<EspionageReport>()
                                                .Where(m => m.Coordinate >= _from.LowerCoordinate && m.Coordinate <= _to.UpperCoordinate);

                Logger.Instance.Log(LogLevel.Info, $"{messages.Count()} Messages parsed, sending ships.");
                if (Strategy.OnBeforeAttack())
                {
                    Resources totalPlunder = Attack(messages);
                    Strategy.OnAfterAttack();
                    Logger.Instance.Log(LogLevel.Info, $"Job done, theoretical total plunder: {totalPlunder}");
                }
                else
                {
                    Logger.Instance.Log(LogLevel.Info, $"Strategy decided not to attack, job done.");
                }
            }
            return null;
            
        }

        /* Extract to IFarmingStrategy
        private IEnumerable<Planet> GetFarmsToScanForFleet()
        {

        }
        */

        private void SendProbes(IEnumerable<Planet> farms)
        {
            Progress = "Sending probes";
            HttpRequestMessage req = RequestBuilder.GetPage(PageType.Galaxy, PlanetId);
            ResponseContainer resp = Client.IssueRequest(req);

            Coordinate self = resp.GetParsedSingle<OgamePageInfo>().PlanetCoord;

            var farmList = farms.OrderBy(f => f.LocationId).ToList();

            int count = farmList.Count;
            int retry = 0;
            int failedInARow = 0;

            foreach (Planet farm in farmList)
            {
                if (farm.Coordinate == self) continue;

                MinifleetResponse minifleet;
                retry = 0;
                do
                {
                    req = RequestBuilder.GetMiniFleetSendMessage(MissionType.Espionage, farm.Coordinate, Strategy.ProbeCount, _token);
                    resp = Client.IssueRequest(req);
                    minifleet = resp.GetParsedSingle<MinifleetResponse>();
                    _token = minifleet.NewToken;
                    
                    // If there are no probes (or error), wait for 5s, otherwise 1s
                    Thread.Sleep((minifleet.Response.Probes > 0 ? 1000 : 5000) + _sleepTime.Next(1000));
                    retry++;
                }
                while (!minifleet.Response.IsSuccess && retry < 5);

                if (minifleet.Response.IsSuccess)
                {
                    failedInARow = 0;
                }
                else
                {
                    Logger.Instance.Log(LogLevel.Error, $"Sending probes to {farm.Coordinate} failed, last error: {minifleet.Response.Message}");
                    if (++failedInARow == 3)
                    {
                        Logger.Instance.Log(LogLevel.Error, $"Failed to send probe {failedInARow} times in a row, aborting further scan");
                        break;
                    }
                }

                if (--count % 10 == 0 && count > 0)
                {
                    Logger.Instance.Log(LogLevel.Info, $"{count} remaining to scan...");
                }
            }
        }

        private void WaitForProbes()
        {
            Progress = "Waiting for probes";
            Thread.Sleep(5000 + _sleepTime.Next(2000));

            IEnumerable<FleetInfo> probes;
            do
            {
                HttpRequestMessage req = RequestBuilder.GetPage(PageType.FleetMovement);
                ResponseContainer resp = Client.IssueRequest(req);

                probes = resp.GetParsed<FleetInfo>().Where(fi => fi.MissionType == MissionType.Espionage && !fi.IsReturning);
                if (probes.Any())
                {
                    Thread.Sleep(3000 + _sleepTime.Next(2000));
                }
            } while (probes.Any());
        }

        private Resources Attack(IEnumerable<EspionageReport> messages)
        {
            Progress = "Attacking";
            //Check if the planet wasn't changed in the meantime (ie. by user action), we'd be sending cargos for a long trip
            ResponseContainer resp = Client.IssueRequest(RequestBuilder.GetPage(PageType.Fleet, PlanetId));
            OgamePageInfo info = resp.GetParsedSingle<OgamePageInfo>();

            Resources totalPlunder = new Resources();
            foreach (var farm in Strategy.GetTargets(messages))
            {
                Thread.Sleep(3000 + _sleepTime.Next(2000));
                totalPlunder += farm.ExpectedPlunder;
                Logger.Instance.Log(LogLevel.Info, $"Attacking planet {farm.Destination} to plunder {farm.ExpectedPlunder}");

                SendFleetCommand attack = new SendFleetCommand()
                {
                    Mission = farm.Mission,
                    Destination = farm.Destination,
                    PlanetId = PlanetId,
                    Fleet = farm.Fleet
                };
                attack.Run();
            }

            return totalPlunder;
        }

        protected override CommandQueueElement RunInternal()
        {
            var req = Client.RequestBuilder.GetPage(PageType.Galaxy, PlanetId);
            var resp = Client.IssueRequest(req);
            var source = resp.GetParsedSingle<OgamePageInfo>();

            // Start scanner
            _from = source.PlanetCoord;
            _from.System = (short)Math.Max(_from.System - Range, 1);

            _to = source.PlanetCoord;
            _to.System = (short)Math.Min(_to.System + Range, 499);

            var scanner = new ScanCommand()
            {
                PlanetId = PlanetId,
                From = _from,
                To = _to
            };
            scanner.Run();
            return Farm();
        }
    }
}
 