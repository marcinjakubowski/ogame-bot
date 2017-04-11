using System.Collections.Generic;
using System.Linq;
using OgameBot.Db;
using OgameBot.Engine.Parsing.Objects;
using OgameBot.Utilities;
using ScraperClientLib.Engine.Parsing;
using System;

namespace OgameBot.Engine.Savers
{
    public class PlayerPlanetSaver : SaverBase
    {
        private bool _isPlayerSeeded = false;
        public override void Run(List<DataObject> result)
        {
            OgamePageInfo current = result.OfType<OgamePageInfo>().FirstOrDefault();

            if (current == null)
                return;

            var playerPlanets = result.OfType<PlanetListItem>();

            PlanetResources resources = result.OfType<PlanetResources>().FirstOrDefault();
            var buildings = result.OfType<DetectedBuilding>().ToDictionary(s => s.Building, s => s.Level);
            var ships = result.OfType<DetectedShip>().ToDictionary(s => s.Ship, s => s.Count);
            var defences = result.OfType<DetectedDefence>().ToDictionary(s => s.Building, s => s.Count);
            var research = result.OfType<DetectedResearch>().ToDictionary(s => s.Research, s => s.Level);

            using (BotDb db = new BotDb())
            {
                long[] locIds = playerPlanets.Select(s => s.Coordinate.Id).ToArray();
                Dictionary<long, Planet> existing = db.Planets.Where(s => locIds.Contains(s.LocationId)).ToDictionary(s => s.LocationId);

                if (!_isPlayerSeeded)
                {
                    if (!db.Players.Where(s => s.PlayerId == current.PlayerId).Any())
                    {
                        db.Players.Add(new Player()
                        {
                            PlayerId = current.PlayerId,
                            Name = current.PlayerName,
                            Status = PlayerStatus.None
                        });
                        db.SaveChanges();
                    }
                    _isPlayerSeeded = true;
                }
                

                foreach (var playerPlanet in playerPlanets)
                {
                    Planet item;

                    if (!existing.TryGetValue(playerPlanet.Coordinate.Id, out item))
                    {
                        item = new Planet()
                        {
                            Coordinate = playerPlanet.Coordinate,
                            Name = playerPlanet.Name,
                            PlanetId = playerPlanet.Id,
                            PlayerId = current.PlayerId
                        };
                        db.Planets.Add(item);
                    }

                    if (resources.Coordinate.Id == playerPlanet.Coordinate.Id)
                    {
                        item.Resources = resources.Resources;
                    }

                    if (current.PlanetId == playerPlanet.Id)
                    {
                        if (ships.Count > 0)
                        {
                            item.Ships.FromPartialResult(ships);
                            item.Ships.LastUpdated = DateTimeOffset.UtcNow;
                        }
                        else if (defences.Count > 0)
                        {
                            item.Defences.FromPartialResult(defences);
                            item.Defences.LastUpdated = DateTimeOffset.UtcNow;
                        }
                        else if (buildings.Count > 0)
                        {
                            item.Buildings.FromPartialResult(buildings);
                            item.Buildings.LastUpdated = DateTimeOffset.UtcNow;
                        }
                        else if (research.Count > 0)
                        {
                            item.Player.Research.FromPartialResult(research);
                            item.Player.Research.LastUpdated = DateTimeOffset.UtcNow;
                        }
                    }
                    item.Name = playerPlanet.Name;
                    item.PlayerId = current.PlayerId;
                    item.PlanetId = playerPlanet.Id;
                    db.SaveChanges();
                }
            }

            List<MessageBase> messages = result.OfType<MessageBase>().ToList();
            if (!messages.Any())
                return;

            int[] messageIds = messages.Select(s => s.MessageId).ToArray();

            using (BotDb db = new BotDb())
            {
                HashSet<int> existing = db.Messages.Where(s => messageIds.Contains(s.MessageId)).Select(s => s.MessageId).ToHashset();

                foreach (MessageBase message in messages.Where(s => !existing.Contains(s.MessageId)))
                {
                    db.Messages.Add(new Message
                    {
                        MessageId = message.MessageId,
                        Body = message,
                        TabType = message.TabType
                    });
                }

                db.SaveChanges();
            }
        }
    }
}
