using OgameBot.Logging;
using System.Collections.Generic;
using OgameBot.Engine.Tasks;

namespace OgameBot
{
    public class Config
    {
        public class HostileWarningSettings
        {
            public string Server { get; set; }
            public string Login { get; set; }
            public string Password { get; set; }
            public string From { get; set; }
            public string To { get; set; }
        }

        public class FarmingSettings
        {
            public int DefaultRange { get; set; }
            public int InactiveMinimumRanking { get; set; }
            public int HuntMaximumRanking { get; set; }
            public int HuntMinimumRanking { get; set; }
            public bool HuntMoonsOnly { get; set; }
            public int HuntProbeCount { get; set; }
        }
            

        public string ListenAddress { get; set; } = "127.0.0.1";
        public int ListenPort { get; set; } = 9400;

        public string Username { get; set; }
        public string Password { get; set; }
        public string Server { get; set; }
        
        public int FleetToRecall { get; set; }

        public FarmingSettings Farming { get; set; }

        public LogLevel LogLevel { get; set; }
        public bool LogIncludeTimestamp { get; set; }

        public HostileWarningSettings HostileWarning { get; set; }
        public int[] CustomOrder { get; set; }
        public SessionKeepAliveMode SessionKeepaliveMode { get; set; } = SessionKeepAliveMode.All;
        public int SessionKeepalivePlanet { get; set; }

        public List<string> SystemsToScan;
    }
}