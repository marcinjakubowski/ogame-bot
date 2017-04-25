using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OgameBot.Utilities
{
    public static class DateTimeOffsetExtensions
    {
        static long TicksPerMinute = TimeSpan.FromMinutes(1).Ticks;

        public static DateTimeOffset TruncateToMinute(this DateTimeOffset self)
        {
            return self.AddTicks(-(self.Ticks % TicksPerMinute));
        }
    }
}
