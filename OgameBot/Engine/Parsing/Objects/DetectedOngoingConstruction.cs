using OgameBot.Objects.Types;
using ScraperClientLib.Engine.Parsing;
using System;

namespace OgameBot.Engine.Parsing.Objects
{
    public class DetectedOngoingConstruction : DataObject
    {
        public BuildingType Building { get; set; }
        public int Level { get; set; }
        public DateTime FinishingAt { get; set; }
        public override string ToString()
        {
            return $"{Building}, lvl {Level:N0}, finishing at {FinishingAt}";
        }
    }
}
