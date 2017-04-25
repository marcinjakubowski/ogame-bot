﻿using System;
using System.Net.Http;
using OgameBot.Objects;
using OgameBot.Objects.Types;
using OgameBot.Utilities;
using System.Collections.Generic;

namespace OgameBot.Engine.Commands
{
    public class OGameRequestBuilder
    {
        private readonly OGameClient _client;

        public OGameRequestBuilder(OGameClient client)
        {
            _client = client;
        }

        public HttpRequestMessage GetPage(PageType page, int? cp = null)
        {
            string link = ((Page)page).Link;
            if (cp != null)
            {
                link += $"&cp={cp}";
            }
            return _client.BuildRequest(new Uri($"/game/index.php?page={link}", UriKind.Relative));
        }

        public HttpRequestMessage PostPage(PageType page, KeyValuePair<string, string>[] postParameters)
        {
            string link = ((Page)page).Link;
            return _client.BuildPost(new Uri($"/game/index.php?page={link}", UriKind.Relative), postParameters);
        }

        public HttpRequestMessage GetEventList()
        {
            return _client.BuildRequest(new Uri("/game/index.php?page=eventList&ajax=1", UriKind.Relative));
        }

        public HttpRequestMessage GetBuildBuildingRequest(BuildingType type, string token)
        {
            return _client.BuildPost(new Uri($"/game/index.php?page=resources&deprecated=1", UriKind.Relative), new []
            {
                KeyValuePair.Create("type", ((int)type).ToString()), // building id
                KeyValuePair.Create("modus", "1"), // 1 = build, 2 = cancel, 3 = demolish
                KeyValuePair.Create("token", token)
            });
        }

        public HttpRequestMessage GetGalaxyContent(SystemCoordinate system)
        {
            return _client.BuildPost(new Uri("/game/index.php?page=galaxyContent&ajax=1", UriKind.Relative), new[]
            {
                KeyValuePair.Create("galaxy", system.Galaxy.ToString()),
                KeyValuePair.Create("system", system.System.ToString())
            });
        }

        public HttpRequestMessage GetMessagePageRequest(MessageTabType tab, int page)
        {
            return _client.BuildPost(new Uri("/game/index.php?page=messages", UriKind.Relative), new[]
            {
                KeyValuePair.Create("messageId", "-1"),
                KeyValuePair.Create("tabid", ((int)tab).ToString()),
                KeyValuePair.Create("action", "107"),
                KeyValuePair.Create("pagination", page.ToString()),
                KeyValuePair.Create("ajax", "1")
            });
        }

        public HttpRequestMessage GetMessagePage(int messageId, MessageTabType tabType)
        {
            return _client.BuildRequest(new Uri($"/game/index.php?page=messages&messageId={messageId}&tabid={(int)tabType}&ajax=1", UriKind.Relative));
        }

        public HttpRequestMessage GetOverviewPage()
        {
            return _client.BuildRequest(new Uri($"/game/index.php?page=overview", UriKind.Relative));
        }

        public HttpRequestMessage GetMiniFleetSendMessage(MissionType mission, Coordinate coordinate, int shipCount, string token)
        {
            return _client.BuildPost(new Uri("/game/index.php?page=minifleet&ajax=1", UriKind.Relative), new[]
            {
                KeyValuePair.Create("mission", ((int)mission).ToString()),
                KeyValuePair.Create("galaxy", coordinate.Galaxy.ToString()),
                KeyValuePair.Create("system", coordinate.System.ToString()),
                KeyValuePair.Create("position", coordinate.Planet.ToString()),
                KeyValuePair.Create("type", ((int)coordinate.Type).ToString()),
                KeyValuePair.Create("shipCount", shipCount.ToString()),
                KeyValuePair.Create("token", token)
            });
        }

        public HttpRequestMessage GetLoginRequest(string server, string username, string password)
        {
            Uri loginUri = new Uri("https://en.ogame.gameforge.com/main/login");

            return _client.BuildPost(loginUri, new[]
            {
                KeyValuePair.Create("kid",string.Empty),
                KeyValuePair.Create("uni", server),
                KeyValuePair.Create("login", username),
                KeyValuePair.Create("pass",password)
            });
        }

        public HttpRequestMessage GetFleetCheckDebris(Coordinate coordinate)
        {
            return _client.BuildPost(new Uri("/game/index.php?page=fleetcheck&ajax=1&espionage=0", UriKind.Relative), new[]
            {
                KeyValuePair.Create("galaxy", coordinate.Galaxy.ToString()),
                KeyValuePair.Create("system", coordinate.System.ToString()),
                KeyValuePair.Create("planet", coordinate.Planet.ToString()),
                KeyValuePair.Create("type", "2"), // debris field, ignore coord
                KeyValuePair.Create("recycler", "1") // make them think we're including a recycler
            });
        }
    }
}