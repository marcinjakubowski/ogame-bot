using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace OgameBot.Engine.Injects
{
    public static class InjectHelper
    {
        public static string EncodeTooltip(string tooltip)
        {
            return HttpUtility.HtmlEncode(tooltip);
        }

        public static string GenerateCommandLink(string command, string label)
        {
            return $@"<a href=""javascript:void(0)"" onclick=ogbcmd(""{command}"")>{label}</a>";
        }
    }
}
