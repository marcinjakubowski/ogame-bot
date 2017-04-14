using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OgameBot.Engine.Parsing.Objects;
using ScraperClientLib.Engine;
using OgameBot.Proxy;

namespace OgameBot.Engine.Injects
{
    public class OGameUrlInject : IInject
    {
        public string Inject(OgamePageInfo info, string body, ResponseContainer response, string host, int port)
        {
            if (info?.Page != null)
                body = body.Replace(@"</body>", $@"<script type=""text/javascript"">ogameUrl = 'http://{host}:{port}';</script></body>");

            body = body.Replace("no-commander", "");

            return body;
        }
    }
}
