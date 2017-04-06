using OgameBot.Objects;
using OgameBot.Objects.Types;

namespace OgameBot.Engine.Tasks.Farming
{
    public class Target
    {
        public Coordinate Destination { get; set; }
        public MissionType Mission { get; set; } = MissionType.Attack;
        public FleetComposition Fleet { get; set; }
        public Resources ExpectedPlunder { get; set; }
    }
}
