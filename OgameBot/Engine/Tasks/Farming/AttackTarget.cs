using OgameBot.Objects;

namespace OgameBot.Engine.Tasks.Farming
{
    public class AttackTarget
    {
        public Coordinate Destination { get; set; }
        public FleetComposition Fleet { get; set; }
        public Resources ExpectedPlunder { get; set; }
    }
}
