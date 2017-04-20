using System;

namespace OgameBot.Engine
{
    public class OGameSettings
    {
        public TimeSpan ServerUtcOffset { get; set; }

        public byte Galaxies { get; set; } = 6;
        public short Systems { get; set; } = 499;
        public int Speed { get; set; } = 1;
    }
}