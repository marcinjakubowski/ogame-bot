using System;

namespace OgameBot.Objects
{
    public class Cost
    {
        public Resources InitialPrice { get; }
        protected float ExponentialCostFactor { get; }

        public Cost(Resources initialPrice, float expFactor)
        {
            InitialPrice = initialPrice;
            ExponentialCostFactor = expFactor;
        }

        public Cost(int metal, int crystal, int deuterium, float expFactor) : this(new Resources(metal, crystal, deuterium), expFactor)
        {

        }

        public Resources ForLevel(int level)
        {
            return new Resources()
            {
                Metal = (int)Math.Floor(InitialPrice.Metal * Math.Pow(ExponentialCostFactor, level - 1)),
                Crystal = (int)Math.Floor(InitialPrice.Crystal * Math.Pow(ExponentialCostFactor, level - 1)),
                Deuterium = (int)Math.Floor(InitialPrice.Deuterium * Math.Pow(ExponentialCostFactor, level - 1))
            };
        }
    }
}
