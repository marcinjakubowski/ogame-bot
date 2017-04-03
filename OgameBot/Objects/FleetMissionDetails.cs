using OgameBot.Objects.Types;
using System;

namespace OgameBot.Objects
{
    public struct FleetMissionDetails
    {
        public MissionType Mission { get; set; }
        public DateTimeOffset ArrivalTime { get; set; }
        public bool IsReturn { get; set; }
    }
}
