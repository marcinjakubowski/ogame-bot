using System.Collections.Generic;

namespace OgameBot.Objects.Types
{
    public abstract class BaseCostEntity<TType, TValue> : BaseEntityType<TType, TValue>
        where TType : struct where TValue : BaseCostEntity<TType, TValue>
    {
        public Resources Cost { get; }

        public BaseCostEntity(TType type, Resources cost) : base(type)
        {
            Cost = cost;
        }
    }
}