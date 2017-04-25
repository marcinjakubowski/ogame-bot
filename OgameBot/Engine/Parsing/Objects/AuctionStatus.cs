using System;
using OgameBot.Objects.Types;
using ScraperClientLib.Engine.Parsing;

namespace OgameBot.Engine.Parsing.Objects
{
    public class AuctionStatus : DataObject
    {
        public ShopItem Item { get; set; } = ShopItem.Unknown;
        public int CurrentBid { get; set; }
        public int BidCount { get; set; }
        public string HighestBidderName { get; set; }
        public int HighestBidderId { get; set; }
        public int MinutesRemaining { get; set; }
        public TimeSpan NextIn { get; set; }

        public override string ToString()
        {
            return $"Auction for {Item} at {CurrentBid} ({ (MinutesRemaining == 0 ? "Completed" : "In progress") })";
        }
    }
}
