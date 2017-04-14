using System.Net.Http;
using OgameBot.Objects;
using OgameBot.Objects.Types;
using System.Linq;
using ScraperClientLib.Engine;
using OgameBot.Engine.Parsing.Objects;
using System.Collections.Generic;
using OgameBot.Logging;
using OgameBot.Db;

namespace OgameBot.Engine.Commands
{
    public class BuildCommand : CommandBase
    {
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

            bool cannotContinue = false;

            if (underConstruction != null)
            {
                Logger.Instance.Log(LogLevel.Warning, $"Building {underConstruction.Building} already under construction on planet {info.PlanetName}, will finish at {underConstruction.FinishingAt}");
                cannotContinue = true;
            }
            else if (cost.Metal > resources.Resources.Metal || cost.Crystal > resources.Resources.Crystal || cost.Deuterium > resources.Resources.Deuterium)
            {
                Logger.Instance.Log(LogLevel.Warning, $"Not enough resources! It would cost {cost} to build {BuildingToBuild} level {currentBuildingLevel + 1}, planet {info.PlanetName} only has {resources.Resources}");
                cannotContinue = true;
            }

            if (!cannotContinue)
            {
                HttpRequestMessage buildReq = Client.RequestBuilder.GetBuildBuildingRequest(BuildingToBuild, token);
                AssistedIssue(buildReq);
            }

            return null;
        }
    }
}