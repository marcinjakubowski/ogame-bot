using Newtonsoft.Json;
using OgameBot.Engine.Parsing.Objects;
using OgameBot.Objects;

public class MinifleetResponseBody
{
    public string Message { get; set; }
    public MinifleetType Type { get; set; }
    public int Slots { get; set; }
    public int Probes { get; set; }
    public int Recyclers { get; set; }
    public int Missiles { get; set; }
    public int ShipsSent { get; set; }

    [JsonConverter(typeof(CoordHelper))]
    public Coordinate Coordinates;

    [JsonProperty("success")]
    public bool IsSuccess { get; set; }
}