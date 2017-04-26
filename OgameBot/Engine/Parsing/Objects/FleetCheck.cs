using OgameBot.Objects;
using OgameBot.Objects.Types;
using ScraperClientLib.Engine.Parsing;

namespace OgameBot.Engine.Parsing.Objects
{
    public class FleetCheck : DataObject
    {
        public FleetCheck()
        {
        }

        public FleetCheck(byte galaxy, short system, byte planet, int type)
        {
            Coords = new Coordinate(galaxy, system, planet, (CoordinateType)type);
        }

        public Coordinate Coords { get; set; }
        public FleetCheckStatus Status { get; set; } = FleetCheckStatus.Unknown;
        
        public override string ToString()
        {
            return Status.ToString();
        }
    }
}
