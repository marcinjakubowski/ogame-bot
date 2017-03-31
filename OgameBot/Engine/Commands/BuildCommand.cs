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

            if (cost.Metal > Where.Resources.Metal || cost.Crystal > Where.Resources.Crystal || cost.Deuterium > Where.Resources.Deuterium)
            {
                Logger.Instance.Log(LogLevel.Error, $"Not enough resources! It would cost {cost} to build {BuildingToBuild}, planet {Where.Name} only has {Where.Resources}");
                return;
            }

            // #todo building in progress check

            HttpRequestMessage buildReq = Client.RequestBuilder.GetBuildBuildingRequest(BuildingToBuild, token);
            AssistedIssue(buildReq);
        }
    }
}