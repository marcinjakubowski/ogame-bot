using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OgameBot.Db;
using OgameBot.Engine.Parsing.Objects;
using OgameBot.Objects;
using OgameBot.Engine.Commands.Farming;

namespace OgameBot.Engine.Commands
{
    public class ScanAroundOwnCommand : CommandBase
    {
        public int Range { get; set; } = 60;

        protected override CommandQueueElement RunInternal()
        {
            var resp = Client.IssueRequest(Client.RequestBuilder.GetOverviewPage());
            var info = resp.GetParsedSingle<OgamePageInfo>();

            int playerId = info.PlayerId;

            using (BotDb db = new BotDb())
            {
                try
                {

                    db.Planets.Where(p => p.PlayerId == playerId)
                              .Select(p => p.LocationId)
                              .AsEnumerable()
                              .Select(p => (Coordinate)p)
                              .Where(c => c.Type != Objects.Types.CoordinateType.Moon)
                              .ToList()
                              .ForEach(Scan);
                }
                catch (Exception ex)
                {
                    Logging.Logger.Instance.LogException(ex);
                }
            }


            return null;
        }

        private void Scan(Coordinate planetCoord)
        {
            SystemCoordinate _from, _to;
            // Start scanner
            _from = planetCoord;
            _from.System = (short)Math.Max(_from.System - Range, 1);

            _to = planetCoord;
            _to.System = (short)Math.Min(_to.System + Range, 499);

            var scanner = new ScanCommand()
            {
                PlanetId = PlanetId,
                From = _from,
                To = _to
            };
            scanner.Run();
        }
    }
}
