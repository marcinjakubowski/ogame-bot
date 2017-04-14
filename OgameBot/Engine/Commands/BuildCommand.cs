using System.Net.Http;
using OgameBot.Objects;
using OgameBot.Objects.Types;
using System.Linq;
using ScraperClientLib.Engine;
using OgameBot.Engine.Parsing.Objects;
using OgameBot.Logging;
using OgameBot.Db;
using System;

namespace OgameBot.Engine.Commands
{
    public class BuildCommand : CommandBase
    {
        private const int MaxRescheduleTimeInMinutes = 15;

        public BuildingType BuildingToBuild { get; set; }

        protected override CommandQueueElement RunInternal()
        {
            // Make the initial request to get a token
            HttpRequestMessage req = Client.RequestBuilder.GetPage(PageType.Resources, PlanetId);
            ResponseContainer res = AssistedIssue(req);

            OgamePageInfo info = res.GetParsedSingle<OgamePageInfo>();
            string token = info.OrderToken;

            // validate resources
            int currentBuildingLevel = res.GetParsed<DetectedBuilding>()
                                          .Where(b => b.Building == BuildingToBuild)
                                          .Select(b => b.Level)
                                          .FirstOrDefault();

            PlanetResources resources = res.GetParsedSingle<PlanetResources>();

            Resources cost = Building.Get(BuildingToBuild).Cost.ForLevel(currentBuildingLevel + 1);
            DetectedOngoingConstruction underConstruction = res.GetParsedSingle<DetectedOngoingConstruction>(false);

            if (underConstruction != null)
            {
                Logger.Instance.Log(LogLevel.Debug, $"Building {underConstruction.Building} already under construction on planet {info.PlanetName}, will finish at {underConstruction.FinishingAt}");
                return Reschedule(underConstruction.FinishingAt);
            }
            else if (cost.Metal > resources.Resources.Metal || cost.Crystal > resources.Resources.Crystal || cost.Deuterium > resources.Resources.Deuterium)
            {
                double maxTimeToGetEnoughResources = new double[] {
                    (cost.Metal - resources.Resources.Metal) / (resources.ProductionPerHour.Metal / 3600.0),
                    (cost.Crystal - resources.Resources.Crystal) / (resources.ProductionPerHour.Deuterium / 3600.0),
                    (cost.Deuterium - resources.Resources.Deuterium) / (resources.ProductionPerHour.Deuterium / 3600.0)
                }.Max();
                Logger.Instance.Log(LogLevel.Debug, $"Not enough resources! It would cost {cost} to build {BuildingToBuild} level {currentBuildingLevel + 1}, planet {info.PlanetName} only has {resources.Resources}; will have enough in {maxTimeToGetEnoughResources} seconds");
                return Reschedule(DateTimeOffset.Now.AddSeconds(maxTimeToGetEnoughResources));
            }

            
            HttpRequestMessage buildReq = Client.RequestBuilder.GetBuildBuildingRequest(BuildingToBuild, token);
            AssistedIssue(buildReq);

            return null;
        }

        private CommandQueueElement Reschedule(DateTimeOffset finishingAt)
        {
            return new CommandQueueElement()
            {
                Command = this,
                ScheduledAt = new[] { finishingAt, DateTimeOffset.Now.AddMinutes(MaxRescheduleTimeInMinutes) }.Min().AddSeconds(5)
            };
        }
    }
}