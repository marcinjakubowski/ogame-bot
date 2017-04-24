using OgameBot.Engine.Parsing.Objects;
using ScraperClientLib.Engine;
using OgameBot.Objects.Types;
using System.Text.RegularExpressions;

namespace OgameBot.Engine.Injects
{
    public class BuildQueueInject : IInject
    {
        private static Regex regex = new Regex(@"(<li id=""button\d+"" class=""(?:disabled|on)"".*?)(buildingimg"">)(.*?)((?<!</a>\s+)<a\s+class="".*?\s+ref=""(\d+)"")", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);


        public string Inject(OgamePageInfo info, string body, ResponseContainer response, string host, int port)
        {
            if (info == null || (info != null && info.Page != PageType.Facilities && info.Page != PageType.Resources)) return body;
            string replace = $@"<a style='filter: hue-rotate(90deg)' class='fastBuild tooltip js_hideTipOnMobile' title='Add to Build Queue' href='javascript:void(0)' onclick=""ogbcmd('build?cp={info.PlanetId}&id=$5')""></a>";
            body = regex.Replace(body, $@"$1$2{replace}$3$4");

            // hack to remove solar satellite
            body = body.Replace(replace.Replace("$5", "212"), string.Empty);

            return body;
        }
    }
}
