using System.Collections.Generic;

namespace OgameBot.Objects.Types
{
    public abstract class BaseGrowingCostEntity<TType, TValue> : BaseEntityType<TType, TValue>
        where TType : struct where TValue : BaseGrowingCostEntity<TType, TValue>
    {
        public Cost Cost { get; }

        public BaseGrowingCostEntity(TType type, Cost cost) : base(type)
        {
            Cost = cost;
        }
    }
}