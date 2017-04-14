using OgameBot.Objects.Types;
using ScraperClientLib.Engine.Parsing;
using System;

namespace OgameBot.Engine.Parsing.Objects
{
    public class DetectedOngoingResearch : DataObject
    {
        public ResearchType Research { get; set; }
        public int Level { get; set; }
        public DateTimeOffset FinishingAt { get; set; } = DateTimeOffset.Now.AddSeconds(5);
        public override string ToString()
        {
            return $"{Research}, lvl {Level:N0}, finishing at {FinishingAt}";
        }
    }
}
