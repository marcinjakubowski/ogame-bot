using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using ScraperClientLib.Engine;
using ScraperClientLib.Utilities;
using OgameBot.Logging;
using System.Collections.Specialized;
using System.Threading.Tasks;

namespace OgameBot.Proxy
{
    public class OgameClientProxy
    {
        public static string ListenHost { get; private set; }
        public static int ListenPort { get; private set; }
        private readonly ClientBase _client;
        private readonly HttpListener _listener;
        private bool _isRunning;

        private readonly Dictionary<string, Action<NameValueCollection>> _commands;

        public Uri SubstituteRoot { get; set; }

        public OgameClientProxy(string listenHost, int listenPort, ClientBase client)
        {
            ListenHost = listenHost;
            ListenPort = listenPort;
            _client = client;
            _listener = new HttpListener();
            _commands = new Dictionary<string, Action<NameValueCollection>>();
            ListenPrefix = $"http://{listenHost}:{listenPort}/";
            _listener.Prefixes.Add(ListenPrefix);
        }
        public static string CommandPrefix { get; set; } = "ogbcmd";

        public string ListenPrefix { get; }

        public void Start()
        {
            _isRunning = true;
            _listener.Start();
            _listener.BeginGetContext(Process, null);
        }

        public void Stop()
        {
            _isRunning = false;
            _listener.Stop();
        }

        private void Process(IAsyncResult ar)
        {
            HttpListenerContext ctx = null;
            try
            {
                ctx = _listener.EndGetContext(ar);
            }
            catch (HttpListenerException ex) when (ex.ErrorCode == 995) // ERROR_OPERATION_ABORTED = 995
            {
                // Request was aborted. Most likely the listener has been stopped
                // Ignore these
            }

            if (_isRunning)
                _listener.BeginGetContext(Process, null);

            if (ctx == null)
                return;

            string referer = ctx.Request.Headers.Get("Referer");
            // Process the current context
            HttpMethod requestedMethod = new HttpMethod(ctx.Request.HttpMethod);
            if (requestedMethod != HttpMethod.Get && requestedMethod != HttpMethod.Post)
            {
                ctx.Response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
                ctx.Response.OutputStream.WriteString($"Unsupported method: {requestedMethod}");
                return;
            }

            Action<string> Redirect = (location) =>
            {
                ctx.Response.StatusCode = (int)HttpStatusCode.TemporaryRedirect;
                ctx.Response.RedirectLocation = location;

                ctx.Response.OutputStream.WriteString("Redirecting to Overview");

                // Return
                ctx.Response.Close();
            };
            string overview = "/game/index.php?page=overview";

            if (ctx.Request.Url.PathAndQuery == "/")
            {
                // Asked for root - send the client to the overview page
                Redirect(overview);
                return;
            }
            else if (ctx.Request.Url.PathAndQuery.StartsWith($"/{CommandPrefix}/"))
            {
                ParseAndRunCommand(ctx.Request.Url.AbsolutePath, ctx.Request.QueryString);
                Redirect(referer != null ? referer : overview);
                return;
            }

            // Prepare uri
            var pathAndQuery = ctx.Request.Url.PathAndQuery;
            if (pathAndQuery.Contains("redir.php"))
            {
                pathAndQuery = pathAndQuery.Replace(Uri.EscapeDataString($"http://{ListenHost}:{ListenPort}/"), Uri.EscapeDataString(SubstituteRoot.ToString()));
            }
            
            Uri targetUri = new Uri(SubstituteRoot, pathAndQuery);

            // NOTE: Enable this to load external ressources through proxy
            //if (targetUri.AbsolutePath.StartsWith("/SPECIAL/"))
            //{
            //    // Request is not for our target. It's for elsewhere
            //    string rest = string.Join("", targetUri.Segments.Skip(2));
            //    rest = rest.Replace(":/", "://");

            //    targetUri = new Uri(rest);
            //}

            // Prepare request
            HttpRequestMessage proxyReq = _client.BuildRequest(targetUri);

            proxyReq.Method = requestedMethod;

            
            if (referer != null)
            {
                referer = referer.Replace($"http://{ListenHost}:{ListenPort}/", SubstituteRoot.ToString());
                proxyReq.Headers.TryAddWithoutValidation("Referer", referer);
            }

            string requestedWith = ctx.Request.Headers.Get("X-Requested-With");
            if (requestedWith != null)
            {
                proxyReq.Headers.TryAddWithoutValidation("X-Requested-With", requestedWith);
            }

            if (requestedMethod == HttpMethod.Post)
            {
                MemoryStream ms = new MemoryStream();
                ctx.Request.InputStream.CopyTo(ms);
                ms.Seek(0, SeekOrigin.Begin);

                proxyReq.Content = new StreamContent(ms);
                if (ctx.Request.ContentType != null)
                {
                    proxyReq.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(ctx.Request.ContentType);
                }

            }

            // Issue
            ResponseContainer resp = _client.IssueRequest(proxyReq);

            byte[] data = resp.ResponseMessage.Content.ReadAsByteArrayAsync().Sync();

            foreach (string encoding in resp.ResponseMessage.Content.Headers.ContentEncoding)
            {
                if (encoding == "gzip")
                {
                    using (var ms = new MemoryStream(data))
                    using (var gzip = new GZipStream(ms, CompressionMode.Decompress))
                    using (var msTarget = new MemoryStream())
                    {
                        gzip.CopyTo(msTarget);

                        data = msTarget.ToArray();
                    }
                }
            }

            // Rewrite html/js
            if (resp.IsHtmlResponse || resp.ResponseMessage.Content?.Headers?.ContentType?.MediaType == "application/x-javascript")
            {
                string str = Encoding.UTF8.GetString(data);

                // NOTE: Enable this to load external ressources through proxy
                //str = Regex.Replace(str, @"(https://gf[\d]+.geo.gfsrv.net/)", $"http://{_listenHost}:{_listenPort}/SPECIAL/$0", RegexOptions.Compiled);

                str = str.Replace(SubstituteRoot.ToString().Replace("/", "\\/"), $@"http:\/\/{ListenHost}:{ListenPort}\/");   // In JS strings
                str = str.Replace(SubstituteRoot.ToString(), $"http://{ListenHost}:{ListenPort}/");   // In links
                str = str.Replace(SubstituteRoot.Host + ":" + SubstituteRoot.Port, $"{ListenHost}:{ListenPort}"); // Without scheme
                str = str.Replace(SubstituteRoot.Host, $"{ListenHost}:{ListenPort}"); // Remainders
                // To make overlays work, there is a check against ogameUrl in javascript
                str = _client.Inject(str, resp);

                data = Encoding.UTF8.GetBytes(str);
            }

            // Write headers
            ctx.Response.StatusCode = (int)resp.StatusCode;

            foreach (KeyValuePair<string, IEnumerable<string>> header in resp.ResponseMessage.Headers)
                foreach (string value in header.Value)
                    ctx.Response.AddHeader(header.Key, value);

            ctx.Response.ContentType = resp.ResponseMessage.Content?.Headers?.ContentType?.ToString();

            // Write content
            try
            {
                ctx.Response.OutputStream.Write(data, 0, data.Length);
                ctx.Response.Close();
            }
            catch (Exception ex)
            {
                Logging.Logger.Instance.LogException(ex);
            }
        }

        private void ParseAndRunCommand(string commandPath, NameValueCollection parameters)
        {
            string[] parts = commandPath.Split('/');
            if (parts.Length != 3 || parts[1] != CommandPrefix)
            {
                throw new ArgumentException($"Invalid command path: {commandPath}!", nameof(commandPath));
            }

            string command = parts[2];

            if (!_commands.ContainsKey(command))
            {
                Logger.Instance.Log(LogLevel.Error, $"No such command - {command}");
                return;
            }

            Task.Factory.StartNew(() => _commands[command](parameters));
        }

        public void AddCommand(string name, Action<NameValueCollection> command)
        {
            _commands[name] = command;
        }
    }
}