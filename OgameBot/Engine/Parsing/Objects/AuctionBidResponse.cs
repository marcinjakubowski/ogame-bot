using ScraperClientLib.Engine.Parsing;
using System.Collections.Generic;
using System;

namespace OgameBot.Engine.Parsing.Objects
{
    public class AuctionBidResponse : DataObject
    {
        public bool Error { get; set; }
        public string Message { get; set; }

        // key => cp
        public Dictionary<string, Resources> PlanetResources { get; set; }

        public int Honor { get; set; }
        public string NewToken { get; set; }

        public override string ToString()
        {
            return $"Bid: {Message}";
        }
        

        public class Resources
        {
            public double Metal { get; set; }
            public double Crystal { get; set; }
            public double Deuterium { get; set; }
        }
    }
}
