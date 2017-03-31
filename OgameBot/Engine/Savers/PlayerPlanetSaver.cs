using System.Collections.Generic;
using System.Linq;
using OgameBot.Db;
using OgameBot.Engine.Parsing.Objects;
using OgameBot.Utilities;
using ScraperClientLib.Engine.Parsing;

namespace OgameBot.Engine.Savers
{
    public class PlayerPlanetSaver : SaverBase
    {
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

            using (BotDb db = new BotDb())
            {
                long[] locIds = playerPlanets.Select(s => s.Coordinate.Id).ToArray();
                Dictionary<long, DbPlanet> existing = db.Planets.Where(s => locIds.Contains(s.LocationId)).ToDictionary(s => s.LocationId);

                foreach (var playerPlanet in playerPlanets)
                {
                    DbPlanet item;

                    if (!existing.TryGetValue(playerPlanet.Coordinate.Id, out item))
                    {
                        item = new DbPlanet()
                        {
                            Coordinate = playerPlanet.Coordinate,
                            Name = playerPlanet.Name,
                            PlanetId = playerPlanet.Id,
                            PlayerId = -1
                        };
                        db.Planets.Add(item);
                    }

                    if (resources.Coordinate.Id == playerPlanet.Coordinate.Id)
                    {
                        item.Resources = resources.Resources;
                    }

                    if (current.PlanetId == playerPlanet.Id)
                    {
                        item.Ships.FromPartialResult(ships);
                        item.Defences.FromPartialResult(defences);
                        item.Buildings.FromPartialResult(buildings);
                    }

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
                    db.Messages.Add(new DbMessage
                    {
                        MessageId = message.MessageId,
                        Message = message,
                        TabType = message.TabType
                    });
                }

                db.SaveChanges();
            }
        }
    }
}
