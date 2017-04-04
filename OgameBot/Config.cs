namespace OgameBot
{
    public class Config
    {
        public string Username { get; set; }

        public string Password { get; set; }

        public string Server { get; set; }

        // Temporary for FarmingBot until a better method is implemeted
        public string FarmPlanet { get; set; }
        public bool ShouldStartFarm { get; set; }
        public int FarmMinimumRanking { get; set; }
        public int FarmRange { get; set; }
    }
}