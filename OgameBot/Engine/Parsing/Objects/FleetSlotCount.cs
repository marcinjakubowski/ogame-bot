using System;
using ScraperClientLib.Engine.Parsing;

namespace OgameBot.Engine.Parsing.Objects
{
    public class FleetSlotCount : DataObject
    {
        public int Current { get; set; }
        public int Max { get; set; }

        public override string ToString()
        {
            return $"Fleets: {Current}/{Max}";
        }
    }
}
