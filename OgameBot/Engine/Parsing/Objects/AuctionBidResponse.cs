using ScraperClientLib.Engine.Parsing;
using System.Collections.Generic;
using System;

namespace OgameBot.Engine.Parsing.Objects
{
    public class AuctionBidResponse : DataObject
    {
        public bool Error { get; set; }
        public string Message { get; set; }
        public string NewToken { get; set; }

        public override string ToString()
        {
            return $"Bid: {Message}";
        }
    }
}
