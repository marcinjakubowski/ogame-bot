using OgameBot.Logging;

namespace OgameBot
{
    public class Config
    {
        public string ListenAddress { get; set; } = "127.0.0.1";
        public int ListenPort { get; set; } = 9400;

        public string Username { get; set; }

        public string Password { get; set; }

        public string Server { get; set; }

        // Temporary for FarmingBot until a better method is implemeted
        public string FarmPlanet { get; set; }
        public bool ShouldStartFarm { get; set; }
        public int FarmMinimumRanking { get; set; }
        public int FarmRange { get; set; }

        public int FleetToRecall { get; set; }

        public LogLevel LogLevel { get; set; }
    }
}