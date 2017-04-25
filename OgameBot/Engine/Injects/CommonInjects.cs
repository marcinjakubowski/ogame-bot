using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OgameBot.Engine.Parsing.Objects;
using ScraperClientLib.Engine;
using OgameBot.Proxy;
using OgameBot.Objects.Types;

namespace OgameBot.Engine.Injects
{
    public class CommonInject : IInject
    {
        const string ogbcmdFunction = @"
function ogbcmd(cmd) {
    $.get('/ogbcmd/'+cmd, function() { fadeBox('OK'); }).fail(function() { fadeBox('Error occured.', 1); });
}
";
        public string Inject(OgamePageInfo info, string body, ResponseContainer response, string host, int port)
        {
            if (info?.Page != null)
            {
                body = body.Replace(@"</body>", $@"<script type=""text/javascript"">
                                                        ogameUrl = 'http://{host}:{port}';
                                                        {ogbcmdFunction};
                                                        </script></body>");
            }

            body = body.Replace("no-commander", "");

            return body;
        }
    }
}
