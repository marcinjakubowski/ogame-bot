using ScraperClientLib.Engine;
using OgameBot.Engine.Parsing.Objects;
using OgameBot.Objects.Types;
using System.Text.RegularExpressions;
using OgameBot.Objects;

namespace OgameBot.Engine.Injects
{
    public class CargosForTransportInject : IInject
    {
        private static Regex largeCargo = new Regex(@"(<span class=""level""><span class=""textlabel"">Large Cargo </span>)(\d+</span>)");
        public string Inject(OgamePageInfo info, string body, ResponseContainer response)
        {
            if (info?.Page != PageType.Fleet) return body;

            int cargosNecessary = FleetComposition.ToTransport(response.GetParsedSingle<PlanetResources>().Resources).Ships[ShipType.LargeCargo];

            return largeCargo.Replace(body, $@"$1<span onclick=""toggleMaxShips('#shipsChosen', 203,{cargosNecessary}); checkIntInput('#ship_203', 0, {cargosNecessary}); checkShips('shipsChosen'); event.stopPropagation(); return false;"" style='color: #aaa'>[{cargosNecessary}]</span> $2");
        }
    }
}

