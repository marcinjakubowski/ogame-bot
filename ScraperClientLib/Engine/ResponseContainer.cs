using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using HtmlAgilityPack;
using ScraperClientLib.Engine.Parsing;
using ScraperClientLib.Utilities;

namespace ScraperClientLib.Engine
{
    public class ResponseContainer
    {
        public HttpRequestMessage RequestMessage { get; }

        public HttpResponseMessage ResponseMessage { get; }
        public Lazy<string> Raw { get; }

        public Lazy<HtmlDocument> ResponseHtml { get; }


        public HttpStatusCode StatusCode => ResponseMessage.StatusCode;

        public bool WasSuccess => ResponseMessage.IsSuccessStatusCode;

        public bool IsHtmlResponse => ResponseMessage.Content?.Headers?.ContentType?.MediaType == "text/html";


        public List<DataObject> ParsedObjects { get; set; }

        public ResponseContainer(HttpRequestMessage requestMessage, HttpResponseMessage responseMessage)
        {
            RequestMessage = requestMessage;
            ResponseMessage = responseMessage;

            ParsedObjects = new List<DataObject>();
            Raw = new Lazy<string>(() =>
            {
                StreamReader sr = new StreamReader(ResponseMessage.Content.ReadAsStream2Async().Sync());
                return sr.ReadToEnd();
            });

            ResponseHtml = new Lazy<HtmlDocument>(() =>
            {
                if (ResponseMessage.Content.Headers.ContentType.MediaType != "text/html")
                {
                    // Not HTML
                    return null;
                }

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(Raw.Value);

                return doc;
            });
        }

        public T GetParsedSingle<T>(bool mandatory = true)
        {
            if (mandatory)
                return GetParsed<T>().Single();

            return GetParsed<T>().SingleOrDefault();
        }

        public IEnumerable<T> GetParsed<T>()
        {
            return ParsedObjects.OfType<T>();
        }

        public Dictionary<string, string> GetInputFields(string ofType = null)
        {
            string selector = ofType == null ? string.Empty : $"[@type='{ofType}']";

            return ResponseHtml.Value.DocumentNode
                      .SelectNodes($"//input{selector}")
                      .ToDictionary(s => s.GetAttributeValue("name", string.Empty),
                                    s => s.GetAttributeValue("value", string.Empty));
        }

        public Dictionary<string, string> GetHiddenFields()
        {
            return GetInputFields("hidden");
        }
    }
}