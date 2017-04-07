﻿using System.Threading;
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
        private string _token;

        public FarmingBot(OGameClient client, int planet, int range, IFarmingStrategy strategy)
        {
            _client = client;
            _planet = planet;
            _range = range;
            _strategy = strategy;

            client.OnResponseReceived += PageChanged;
        }

        private void PageChanged(ResponseContainer resp)
        {
            string minifleetToken = resp.GetParsedSingle<OgamePageInfo>(false)?.MiniFleetToken;
            if (minifleetToken != null)
            {
                _token = minifleetToken;
            }
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

        }
        */

        private void SendProbes(IEnumerable<Planet> farms)
        {
            HttpRequestMessage req = RequestBuilder.GetPage(PageType.Galaxy, _planet);
            ResponseContainer resp = _client.IssueRequest(req);

            int count = farms.Count();
            int retry = 0;
            int failedInARow = 0;

            foreach (Planet farm in farms)
            {
                MinifleetResponse minifleet;
                retry = 0;
                do
                {
                    req = RequestBuilder.GetMiniFleetSendMessage(MissionType.Espionage, farm.Coordinate, _strategy.GetProbeCountForTarget(farm), _token);
                    resp = _client.IssueRequest(req);
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
                    Mission = farm.Mission,
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
 