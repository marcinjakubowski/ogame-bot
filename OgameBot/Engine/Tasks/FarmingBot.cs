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
            List<Planet> farms;
            using (BotDb db = new BotDb())
            {
                farms = db.Planets.Where(s => 
                        s.LocationId >= _from.Id && s.LocationId <= _to.Id 
                     && (s.Player.Status.HasFlag(PlayerStatus.Inactive) || s.Player.Status.HasFlag(PlayerStatus.LongInactive))
                     && !s.Player.Status.HasFlag(PlayerStatus.Vacation)
                     && !s.Player.Status.HasFlag(PlayerStatus.Admin)
                     && s.Player.Ranking < _minRanking
                ).ToList();
            }
            Logger.Instance.Log(LogLevel.Info, $"Got {farms.Count} farms");
            foreach (Planet planet in farms)
            {
                Logger.Instance.Log(LogLevel.Debug, $"\t{planet.Coordinate} - {planet.Name}");
            }

            HttpRequestMessage req = RequestBuilder.GetPage(PageType.Galaxy);
            ResponseContainer resp = _client.IssueRequest(req);

            var info = resp.GetParsedSingle<OgamePageInfo>();
            if (info.PlanetName != _planet)
            {
                throw new ApplicationException("Not where we should be");
            }

            Random sleepTime = new Random();

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

/* #todo parse to class instead of jobject
{  
   "response":{  
      "message":"Send espionage probe to:",
      "type":1,
      "slots":8,
      "probes":18,
      "recyclers":0,
      "missiles":0,
      "shipsSent":2,
      "coordinates":{  
         "galaxy":3,
         "system":55,
         "position":8
      },
      "planetType":1,
      "success":true
   },
   "newToken":"fb74477ff26fd4ba9c41c110aa295baf"
}
*/
                    JObject result = JObject.Parse(resp.Raw.Value);
                    wasSuccessful = (bool)result["response"]["success"];
                    if (!wasSuccessful)
                    {
                        retry++;
                        Thread.Sleep(2000 + sleepTime.Next(4000));
                    }
                    token = result["newToken"].ToString();
                    Thread.Sleep(1000 + sleepTime.Next(1000));
                }

                if (!wasSuccessful)
                {
                    Logger.Instance.Log(LogLevel.Error, $"Sending probes to ${farm.Coordinate} failed.");
                }
            }
            Logger.Instance.Log(LogLevel.Info, "Sending probes finished, waiting to come back and read messages");
            // Wait for all the probes to come back
            Thread.Sleep(15000 + sleepTime.Next(5000));

            ReadAllMessagesCommand cmd = new ReadAllMessagesCommand(_client);
            cmd.Run();

            Logger.Instance.Log(LogLevel.Info, "Messages parsed, job done.");
            
        }
    }
}
 