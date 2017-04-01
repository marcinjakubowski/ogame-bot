using System.Net.Http;
using OgameBot.Objects;
using OgameBot.Objects.Types;
using OgameBot.Db;
using System;
using ScraperClientLib.Engine;
using OgameBot.Engine.Parsing.Objects;
using System.Collections.Generic;
using OgameBot.Logging;

namespace OgameBot.Engine.Commands
{
    public class BuildCommand : CommandBase
    {
        public Planet Where { get; }
        public BuildingType BuildingToBuild { get; }

        public BuildCommand(OGameClient client, Planet where, BuildingType building)
            : base(client)
        {
            if (!where.PlanetId.HasValue)
            {
                throw new ArgumentException("Planet must have planetid", nameof(where));
            }

            Where = where;
            BuildingToBuild = building;
        }

        public override void Run()
        {
            // Make the initial request to get a token
            HttpRequestMessage req = Client.RequestBuilder.GetPage(PageType.Resources, Where.PlanetId);
            ResponseContainer res = AssistedIssue(req);
            string token = res.GetParsedSingle<OgamePageInfo>().BuildToken;

            // validate resources
            Dictionary<BuildingType, int> currentBuildings = Where.Buildings;
            Resources cost = Building.Get(BuildingToBuild).Cost.ForLevel(currentBuildings[BuildingToBuild] + 1);
            DetectedOngoingConstruction underConstruction = res.GetParsedSingle<DetectedOngoingConstruction>(false);

            bool cannotContinue = false;

            if (underConstruction != null)
            {
                Logger.Instance.Log(LogLevel.Warning, $"Building {underConstruction.Building} already under construction on planet {Where.Name}, will finish at {underConstruction.FinishingAt}");
                cannotContinue = true;
            }
            else if (cost.Metal > Where.Resources.Metal || cost.Crystal > Where.Resources.Crystal || cost.Deuterium > Where.Resources.Deuterium)
            {
                Logger.Instance.Log(LogLevel.Warning, $"Not enough resources! It would cost {cost} to build {BuildingToBuild}, planet {Where.Name} only has {Where.Resources}");
                cannotContinue = true;
            }

            if (!cannotContinue)
            {
                HttpRequestMessage buildReq = Client.RequestBuilder.GetBuildBuildingRequest(BuildingToBuild, token);
                AssistedIssue(buildReq);
            }
        }
    }
}