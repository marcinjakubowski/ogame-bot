using Newtonsoft.Json;
using OgameBot.Engine.Parsing.Objects;
using OgameBot.Objects;
using OgameBot.Objects.Types;

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

    public CoordinateType PlanetType
    {
        set => Coordinates.Type = value;
        get => Coordinates.Type;
    }

    [JsonProperty("success")]
    public bool IsSuccess { get; set; }
}