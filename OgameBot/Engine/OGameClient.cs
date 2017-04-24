using System;
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
using System.Threading;
using OgameBot.Engine.RequestValidation;
using System.Net;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace OgameBot.Engine
{
    public class OGameClient : ClientBase
    {
        private readonly string _server;
        private readonly string _username;
        private readonly string _password;

        private readonly List<SaverBase> _savers;
        private readonly List<IInject> _injects;
        private readonly List<IRequestValidator> _validators;

        private object _lockPlanetExclusive = new object();
        private CookieContainer _cookieContainer;

        private const string _cookiePath = "temp/cookies.bin";

        public PlanetExclusiveOperation CurrentPlanetExclusiveOperation { get; private set; } = null;

        public event Action<ResponseContainer> OnResponseReceived;

        public static OGameClient Instance { get; private set; }

        public OGameStringProvider StringProvider { get; }

        public OGameSettings Settings { get; }

        public OGameRequestBuilder RequestBuilder { get; }

        public CommandBase.Commander Commander { get; }

        public OGameClient(string server, OGameStringProvider stringProvider, string username, string password, CommandBase.Commander commander)
        {
            _server = server;
            _username = username;
            _password = password;

            _savers = new List<SaverBase>();
            _injects = new List<IInject>();
            _validators = new List<IRequestValidator>();

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
            RegisterParser(new MinifleetParser());

            RegisterIntervention(new OGameAutoLoginner(this));
            RegisterValidator(new PlanetExclusiveValidator(this));

            if (Instance != null)
            {
                throw new ApplicationException($"Only one {nameof(OGameClient)} instance can exist.");
            }

            Instance = this;
            Commander = commander;
            
        }

        public void RegisterInject(IInject inject)
        {
            using (EnterExclusive())
                _injects.Add(inject);
        }
        public void RegisterValidator(IRequestValidator validator)
        {
            using (EnterExclusive())
                _validators.Add(validator);
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

        public override ResponseContainer IssueRequest(HttpRequestMessage request)
        {
            foreach (var validator in _validators)
            {
                request = validator.Validate(request);
            }
            return base.IssueRequest(request);
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

        protected override CookieContainer GetCookieContainer()
        {
            _cookieContainer = null;
            FileInfo cookiesFile = new FileInfo(_cookiePath);

            if (!cookiesFile.Exists)
            {
                _cookieContainer = base.GetCookieContainer();
            }
            else
            {
                BinaryFormatter formatter = new BinaryFormatter();
                _cookieContainer = (CookieContainer)formatter.Deserialize(cookiesFile.OpenRead());
            }

            return _cookieContainer;
        }

        public void SaveCookies()
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileInfo fi = new FileInfo(_cookiePath);
            using (var fs = fi.OpenWrite())
                formatter.Serialize(fs, _cookieContainer);
        }
                
        public override string Inject(string body, ResponseContainer response, string host, int port)
        {
            OgamePageInfo info = response.GetParsedSingle<OgamePageInfo>(false);
            foreach (IInject inject in _injects)
                body = inject.Inject(info, body, response, host, port);

            return base.Inject(body, response, host, port);
        }

        public IDisposable EnterPlanetExclusive(IPlanetExclusiveOperation operation)
        {
            Monitor.Enter(_lockPlanetExclusive);
            // if the same thread entered here, but invoked an exlusive planet operation for a different planet, it's an error!
            if (CurrentPlanetExclusiveOperation != null)
            {
                // the same planet id, it's all fine, we don't need a new lock because the existing one is still there
                if (CurrentPlanetExclusiveOperation.Operation.PlanetId == operation.PlanetId)
                {
                    return null;
                }
                else
                {
                    Logger.Instance.Log(LogLevel.Error, $"Current planet exclusive is not released!!! Current: {CurrentPlanetExclusiveOperation.Operation.Name}, new: {operation.Name}.");
                }
            }
            CurrentPlanetExclusiveOperation = new PlanetExclusiveOperation(this, operation);
            return CurrentPlanetExclusiveOperation;
        }

        public void LeavePlanetExclusive(PlanetExclusiveOperation op)
        {
            if (op == CurrentPlanetExclusiveOperation)
            {
                op.Dispose();
            }
            else
            {
                throw new ArgumentException("Invalid planet exclusive token!");
            }
        }

        public class PlanetExclusiveOperation : IDisposable
        {
            private OGameClient _client;
            public IPlanetExclusiveOperation Operation { get; }

            public PlanetExclusiveOperation(OGameClient client, IPlanetExclusiveOperation operation)
            {
                _client = client;
                Operation = operation;
                Logger.Instance.Log(LogLevel.Debug, $"Creating PEO {Operation.Name} on planet {Operation.PlanetId}");
            }

            public void Dispose()
            {
                Logger.Instance.Log(LogLevel.Debug, $"Releasing PEO {Operation.Name} on planet {Operation.PlanetId}");
                _client.CurrentPlanetExclusiveOperation = null;
                Monitor.Exit(_client._lockPlanetExclusive);
            }
        }
    }
}