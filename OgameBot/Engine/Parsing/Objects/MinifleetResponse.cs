using ScraperClientLib.Engine.Parsing;

namespace OgameBot.Engine.Parsing.Objects
{
    public class MinifleetResponse : DataObject
    {
        public MinifleetResponseBody Response { get; set; }
        public string NewToken { get; set; }

        public override string ToString()
        {
            if (Response.IsSuccess)
            {
                string what;
                switch (Response.Type)
                {
                    case MinifleetType.Espionage:
                        what = "Espionage Probe";
                        break;
                    case MinifleetType.Recycle:
                        what = "Recycler";
                        break;
                    case MinifleetType.Missile:
                        what = "Missile";
                        break;
                    default:
                        what = "Unknown";
                        break;
                }

                return $"Minifleet OK: {Response.ShipsSent}x {what} -> {Response.Coordinates}";
            }
            
            return $"Minifleet Error: {Response.Message}";
        }
    }


}
