namespace OgameBot.Engine.Parsing.Objects
{
    public class EventInfo : FleetInfo
    {
        public EventType Type { get; set; }

        public override string ToString()
        {
            return $"{Type.ToString()} {base.ToString()}";
        }
    }
}
