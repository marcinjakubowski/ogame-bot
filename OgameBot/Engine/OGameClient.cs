﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Net.Http;
using OgameBot.Engine.Commands;
using OgameBot.Engine.Interventions;
using OgameBot.Engine.Parsing;
using OgameBot.Engine.Savers;
using OgameBot.Logging;
using ScraperClientLib.Engine;
using ScraperClientLib.Engine.Parsing;
using OgameBot.Engine.Parsing.Objects;
using OgameBot.Engine.Injects;

namespace OgameBot.Engine
{
    public class OGameClient : ClientBase
    {
        private readonly string _server;
        private readonly string _username;
        private readonly string _password;

        private readonly List<SaverBase> _savers;
        private readonly List<IInject> _injects;

        public event Action<ResponseContainer> OnResponseReceived;

        public OGameStringProvider StringProvider { get; }

        public OGameSettings Settings { get; }

        public OGameRequestBuilder RequestBuilder { get; }

        public OGameClient(string server, OGameStringProvider stringProvider, string username, string password)
        {
            _server = server;
            _username = username;
            _password = password;

            _savers = new List<SaverBase>();
            _injects = new List<IInject>();

            RequestBuilder = new OGameRequestBuilder(this);

            StringProvider = stringProvider;
            BaseUri = new Uri($"https://{server}/");

            Settings = new OGameSettings();

            RegisterParser(new PageInfoParser());
            RegisterParser(new DefencesPageParser());
            RegisterParser(new FacilitiesPageParser());
            RegisterParser(new FleetMovementPageParser());
            RegisterParser(new GalaxyPageParser());
            RegisterParser(new PlanetListParser());
            RegisterParser(new PlanetResourcesParser());
            RegisterParser(new ResearchPageParser());
            RegisterParser(new ResourcesPageParser());
            RegisterParser(new ShipyardPageParser());
            RegisterParser(new FleetPageParser());
            RegisterParser(new MessagesPageParser());
            RegisterParser(new EspionageDetailsParser());
            RegisterParser(new MessageCountParser());
            RegisterParser(new OngoingActivityParser());
            RegisterParser(new EventListParser());

            RegisterIntervention(new OGameAutoLoginner(this));
        }

        public void RegisterInject(IInject inject)
        {
            using (EnterExclusive())
                _injects.Add(inject);
        }

        public void RegisterSaver(SaverBase saver)
        {
            using (EnterExclusive())
                _savers.Add(saver);
        }

        public IReadOnlyList<IInject> GetInjects()
        {
            return _injects.AsReadOnly();
        }

        public IReadOnlyList<SaverBase> GetSavers()
        {
            return _savers.AsReadOnly();
        }

        protected override void PostRequest(ResponseContainer response)
        {
            Logger.Instance.Log(LogLevel.Debug, $"Got {response.StatusCode} to {response.RequestMessage.RequestUri}, ({response.ParsedObjects.Count:N0} parsed objects)");

            Debug.WriteLine($"Response to {response.RequestMessage.RequestUri}, ({response.ParsedObjects.Count:N0} parsed objects)");
            foreach (DataObject dataObject in response.ParsedObjects)
            {
                Debug.WriteLine($"Parsed object by {dataObject.ParserType}: {dataObject}");
            }

            // Save to DB
            foreach (SaverBase saver in _savers)
            {
                saver.Run(response.ParsedObjects);
            }

            // Execute other interests
            OnResponseReceived?.Invoke(response);
        }

        internal HttpRequestMessage PrepareLogin()
        {
            return RequestBuilder.GetLoginRequest(_server, _username, _password);
        }

        public void PerformLogin()
        {
            using (EnterExclusive())
            {
                HttpRequestMessage loginReq = PrepareLogin();
                IssueRequest(loginReq);
            }
        }

        public override string Inject(string body, ResponseContainer response)
        {
            OgamePageInfo info = response.GetParsedSingle<OgamePageInfo>(false);
            foreach (IInject inject in _injects)
                body = inject.Inject(info, body, response);

            return base.Inject(body, response);
        }
    }
}