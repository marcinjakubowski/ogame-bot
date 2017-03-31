using System;
using System.Collections.Generic;
using System.Linq;
using OgameBot.Db;
using OgameBot.Engine.Parsing.Objects;
using OgameBot.Utilities;
using ScraperClientLib.Engine.Parsing;

namespace OgameBot.Engine.Savers
{
    public class GalaxyPageSaver : SaverBase
    {
        public override void Run(List<DataObject> result)
        {
            GalaxyPageSystem systemDetails = result.OfType<GalaxyPageSystem>().FirstOrDefault();
            if (systemDetails == null)
                return;

            using (BotDb db = new BotDb())
            {
                int systemId = systemDetails.System;
                GalaxyScan scanInfo = db.Scans.SingleOrDefault(s => s.LocationId == systemId);

                if (scanInfo == null)
                {
                    scanInfo = new GalaxyScan
                    {
                        SystemCoordinate = systemDetails.System
                    };

                    db.Scans.Add(scanInfo);
                }

                scanInfo.LastScan = DateTimeOffset.Now;

                // Prep players
                int[] playerIds = result.OfType<GalaxyPageInfoItem>().Select(s => s.PlayerId).ToArray();
                Dictionary<int, Player> players = db.Players.Where(s => playerIds.Contains(s.PlayerId)).ToDictionary(s => s.PlayerId);

                // Individual items
                long systemLower = systemDetails.System.LowerCoordinate;
                long systemUpper = systemDetails.System.UpperCoordinate;

                Dictionary<long, Planet> toRemove = db.Planets.Where(s => systemLower <= s.LocationId && s.LocationId <= systemUpper).ToDictionary(s => s.LocationId);

                foreach (GalaxyPageInfoItem row in result.OfType<GalaxyPageInfoItem>())
                {
                    Player player;
                    if (!players.TryGetValue(row.PlayerId, out player))
                    {
                        player = new Player
                        {
                            PlayerId = row.PlayerId
                        };

                        db.Players.Add(player);
                        players[row.PlayerId] = player;
                    }

                    player.Name = row.PlayerName;
                    player.Status = row.PlayerStatus;

                    Planet planet;
                    if (!toRemove.TryRemove(row.Planet.Coordinate, out planet))
                    {
                        planet = new Planet
                        {
                            Coordinate = row.Planet.Coordinate
                        };

                        db.Planets.Add(planet);
                    }

                    planet.Name = row.Planet.Name;
                    planet.Player = player;

                    if (row.Moon != null)
                    {
                        Planet moon;
                        if (!toRemove.TryRemove(row.Moon.Coordinate, out moon))
                        {
                            moon = new Planet
                            {
                                Coordinate = row.Moon.Coordinate
                            };

                            db.Planets.Add(moon);
                        }

                        moon.Name = row.Moon.Name;
                        moon.Player = player;
                    }
                }

                db.Planets.RemoveRange(toRemove.Values);
                toRemove.Clear();

                db.SaveChanges();
            }
        }
    }
}