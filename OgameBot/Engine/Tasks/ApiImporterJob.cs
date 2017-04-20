using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using OgameApi.Objects;
using OgameApi.Utilities;
using OgameBot.Db;
using OgameBot.Engine.Parsing.Objects;
using OgameBot.Logging;
using OgameBot.Objects;
using OgameBot.Objects.Types;
using ScraperClientLib.Utilities;

namespace OgameBot.Engine.Tasks
{
    public class ApiImporterJob : WorkerBase
    {
        private readonly OGameClient _client;
        private readonly DirectoryInfo _baseDir;

        public ApiImporterJob(OGameClient client, DirectoryInfo baseDir)
        {
            _client = client;
            _baseDir = baseDir;

            if (!_baseDir.Exists)
            {
                _baseDir.Create();
                _baseDir.Refresh();
            }

            ExecutionInterval = TimeSpan.FromHours(1);
        }

        protected override void RunInternal()
        {
            Uri universeUri = new Uri(_client.BaseUri, "/api/universe.xml");
            Uri alliancesUri = new Uri(_client.BaseUri, "/api/alliances.xml");
            Uri playersUri = new Uri(_client.BaseUri, "/api/players.xml");
            Uri serverDataUri = new Uri(_client.BaseUri, "/api/serverData.xml");
            Uri highscoreUri = new Uri(_client.BaseUri, "/api/highscore.xml?category=1&type=0"); // player highscore total

            FileInfo universeFile = new FileInfo(Path.Combine(_baseDir.FullName, _client.BaseUri.Host + "-universe.xml"));
            FileInfo alliancesFile = new FileInfo(Path.Combine(_baseDir.FullName, _client.BaseUri.Host + "-alliances.xml"));
            FileInfo playersFile = new FileInfo(Path.Combine(_baseDir.FullName, _client.BaseUri.Host + "-players.xml"));
            FileInfo serverDataFile = new FileInfo(Path.Combine(_baseDir.FullName, _client.BaseUri.Host + "-serverData.xml"));
            FileInfo highscoreFile = new FileInfo(Path.Combine(_baseDir.FullName, _client.BaseUri.Host + "-highscore.xml"));

            // Always update server data
            {
                Logger.Instance.Log(LogLevel.Info, "ApiImporterJob: Updating serverData from API");
                Update(serverDataUri, serverDataFile).Sync();
                ServerData model = XmlModelSerializer.Deserialize<ServerData>(serverDataFile);
                ProcessData(model);
            }


            if (NeedUpdate(universeUri, universeFile, "universe").Sync())
            {
                Logger.Instance.Log(LogLevel.Info, "ApiImporterJob: Updating universe from API");

                Update(universeUri, universeFile).Sync();

                Universe model = XmlModelSerializer.Deserialize<Universe>(universeFile);
                ProcessData(model);
            }

            if (NeedUpdate(alliancesUri, alliancesFile, "alliances").Sync())
            {
                Logger.Instance.Log(LogLevel.Info, "ApiImporterJob: Updating alliances from API");

                Update(alliancesUri, alliancesFile).Sync();

                AlliancesContainer model = XmlModelSerializer.Deserialize<AlliancesContainer>(alliancesFile);
                ProcessData(model);
            }

            if (NeedUpdate(playersUri, playersFile, "players").Sync())
            {
                Logger.Instance.Log(LogLevel.Info, "ApiImporterJob: Updating players from API");

                Update(playersUri, playersFile).Sync();

                PlayersContainer model = XmlModelSerializer.Deserialize<PlayersContainer>(playersFile);
                ProcessData(model);
            }

            
            
            if (NeedUpdate(highscoreUri, highscoreFile, "highscore").Sync())
            {
                Logger.Instance.Log(LogLevel.Info, "ApiImporterJob: Updating highscore from API");

                Update(highscoreUri, highscoreFile).Sync();

                HighscoreContainer model = XmlModelSerializer.Deserialize<HighscoreContainer>(highscoreFile);
                ProcessData(model);
            }
        }

        private void ProcessData(HighscoreContainer model)
        {
            using (BotDb db = new BotDb())
            {
                Dictionary<int, Db.Player> allPlayers = db.Players.ToDictionary(s => s.PlayerId);

                for (int i=0; i < model.Highscores.Length; i++)
                {
                    var highscore = model.Highscores[i];
                    Db.Player dbPlayer;
                    if (allPlayers.TryGetValue(highscore.Id, out dbPlayer))
                    {
                        dbPlayer.Ranking = highscore.Position;

                        if (i % 250 == 0)
                        {
                            db.SaveChanges();
                        }
                    }
                }
                db.SaveChanges();
            }
        }

        private void ProcessData(AlliancesContainer model)
        {
            

        }

        private void ProcessData(PlayersContainer model)
        {
            using (BotDb db = new BotDb())
            {
                Dictionary<int, Db.Player> allPlayers = db.Players.ToDictionary(s => s.PlayerId);
                List<Db.Player> newPlayers = new List<Db.Player>();

                for (int i = 0; i < model.Players.Length; i++)
                {
                    var player = model.Players[i];
                    Db.Player dbPlayer;
                    if (!allPlayers.TryGetValue(player.Id, out dbPlayer))
                    {
                        dbPlayer = new Db.Player
                        {
                            PlayerId = player.Id
                        };

                        newPlayers.Add(dbPlayer);
                        allPlayers[player.Id] = dbPlayer;
                    }

                    dbPlayer.Name = player.Name;
                    dbPlayer.Status = ParseStatus(player.Status);

                    if (i % 250 == 0)
                    {
                        db.Players.AddRange(newPlayers);

                        db.SaveChanges();

                        newPlayers.Clear();
                    }
                }

                db.Players.AddRange(newPlayers);

                db.SaveChanges();
            }
        }

        private void ProcessData(ServerData model)
        {
            _client.Settings.ServerUtcOffset = TimeSpan.Parse(model.TimezoneOffset.Replace("+", ""));
            _client.Settings.Galaxies = (byte)model.Galaxies;
            _client.Settings.Systems = (short)model.Systems;
            _client.Settings.Speed = model.Speed;
        }

        private void ProcessData(Universe model)
        {
            using (BotDb db = new BotDb())
            {
                Dictionary<long, Db.Planet> allPlanets = db.Planets.ToDictionary(s => s.LocationId);
                Dictionary<int, Db.Player> allPlayers = db.Players.ToDictionary(s => s.PlayerId);

                List<Db.Planet> newPlanets = new List<Db.Planet>();
                List<Db.Player> newPlayers = new List<Db.Player>();

                for (int i = 0; i < model.Planets.Length; i++)
                {
                    OgameApi.Objects.Planet planet = model.Planets[i];
                    Coordinate planetCoords = Coordinate.Parse(planet.Coords, CoordinateType.Planet);

                    Db.Planet dbPlanet;
                    if (!allPlanets.TryGetValue(planetCoords.Id, out dbPlanet))
                    {
                        dbPlanet = new Db.Planet
                        {
                            Coordinate = planetCoords
                        };

                        newPlanets.Add(dbPlanet);
                        allPlanets[planetCoords.Id] = dbPlanet;
                    }

                    dbPlanet.Name = planet.Name;
                    dbPlanet.PlayerId = planet.Player;

                    if (planet.Moon != null)
                    {
                        Coordinate moonCoords = Coordinate.Create(planetCoords, CoordinateType.Moon);

                        Db.Planet dbMoon;
                        if (!allPlanets.TryGetValue(moonCoords.Id, out dbMoon))
                        {
                            dbMoon = new Db.Planet
                            {
                                Coordinate = moonCoords
                            };

                            newPlanets.Add(dbMoon);
                            allPlanets[moonCoords.Id] = dbMoon;
                        }

                        dbMoon.Name = planet.Moon.Name;
                    }

                    Db.Player dbPlayer;
                    if (!allPlayers.TryGetValue(planet.Player, out dbPlayer))
                    {
                        dbPlayer = new Db.Player
                        {
                            PlayerId = planet.Player
                        };

                        newPlayers.Add(dbPlayer);
                        allPlayers[dbPlayer.PlayerId] = dbPlayer;
                    }

                    if (i % 250 == 0)
                    {
                        db.Planets.AddRange(newPlanets);
                        db.Players.AddRange(newPlayers);

                        db.SaveChanges();

                        newPlanets.Clear();
                        newPlayers.Clear();
                    }
                }

                db.Planets.AddRange(newPlanets);
                db.Players.AddRange(newPlayers);

                db.SaveChanges();
            }
        }

        private static async Task Update(Uri uri, FileInfo file)
        {
            using (HttpClient wc = new HttpClient())
            using (Stream ws = await wc.GetStreamAsync(uri))
            using (FileStream fs = file.Open(FileMode.Create, FileAccess.ReadWrite, FileShare.Read))
                await ws.CopyToAsync(fs);
        }

        private static async Task<bool> NeedUpdate(Uri uri, FileInfo file, string rootElement)
        {
            if (!file.Exists)
                return true;

            if (DateTime.UtcNow - file.LastWriteTimeUtc < TimeSpan.FromMinutes(5))
                // File was last written to less than 5 minutes ago
                return false;

            DateTime serverTime = await GetServerDetails(uri);
            DateTime current = GetXmlTimestamp(file, rootElement);

            if (serverTime - current > TimeSpan.FromHours(1))
            {
                // Update
                return true;
            }

            return false;
        }

        private static DateTime GetXmlTimestamp(FileInfo file, string rootElement)
        {
            if (!file.Exists)
                return DateTime.MinValue;

            try
            {
                using (FileStream fs = file.OpenRead())
                using (XmlReader reader = XmlReader.Create(fs))
                {
                    reader.ReadToFollowing(rootElement);
                    long timestamp = long.Parse(reader.GetAttribute("timestamp"));

                    return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(timestamp);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unable to fetch timestamp for {file.FullName}: {ex.Message}");
                return DateTime.MinValue;
            }
        }

        private static async Task<DateTime> GetServerDetails(Uri uri)
        {
            try
            {
                HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Head, uri);

                using (HttpClient client = new HttpClient())
                {
                    using (HttpResponseMessage resp = await client.SendAsync(msg, HttpCompletionOption.ResponseHeadersRead))
                    {
                        DateTimeOffset? lastModified = resp.Content.Headers.LastModified;

                        return lastModified?.UtcDateTime ?? DateTime.MaxValue;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unable to HEAD {uri} (General): {ex.Message}");
            }

            return DateTime.MaxValue;
        }

        private static PlayerStatus ParseStatus(string status)
        {
            PlayerStatus result = PlayerStatus.None;

            if (string.IsNullOrEmpty(status))
                return result;

            foreach (char part in status)
            {
                switch (part)
                {
                    case 'v':
                        result |= PlayerStatus.Vacation;
                        break;
                    case 'b':
                        result |= PlayerStatus.Banned;
                        break;
                    case 'i':
                        result |= PlayerStatus.Inactive;
                        break;
                    case 'I':
                        result |= PlayerStatus.LongInactive;
                        break;
                    case 'o':
                        result |= PlayerStatus.Outlaw;
                        break;
                    case 'a':
                        result |= PlayerStatus.Admin;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return result;
        }
    }
}