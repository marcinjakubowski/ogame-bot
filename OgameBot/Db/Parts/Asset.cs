using System;
using System.Collections.Generic;
using System.Linq;

namespace OgameBot.Db.Parts
{
    public abstract class Asset<T> where T : struct
    {
        public DateTimeOffset? LastUpdated { get; set; }

        public bool NeedsUpdate(DateTimeOffset now)
        {
            return !LastUpdated.HasValue || now > LastUpdated;
        }

        public Asset()
        {
            _dict = new Dictionary<T, int>();
        }

        protected Asset(Dictionary<T, int> other)
        {
            _isSet = true;
            if (other != null) _dict = other;
            else _dict = new Dictionary<T, int>();
        }

        public void FromPartialResult(Dictionary<T, int> dict)
        {
            dict.ToList().ForEach(x => _dict[x.Key] = x.Value);
            _isSet = true;
        }

        protected int? TryGet(T type)
        {
            if (!_isSet) return null;
            if (!_dict.ContainsKey(type)) return 0;

            return _dict[type];
        }

        protected void TrySet(T type, int? value)
        {
            _isSet = true;
            if (!value.HasValue) return;

            _dict[type] = (int)value;
        }

        protected Dictionary<T, int> _dict;
        private bool _isSet = false;
    }
}
