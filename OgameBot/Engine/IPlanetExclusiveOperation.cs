using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OgameBot.Engine
{
    public interface IPlanetExclusiveOperation
    {
        [JsonIgnore]
        int PlanetId { get; }
        [JsonIgnore]
        string Name { get; }
        [JsonIgnore]
        string Progress { get; }
    }
}
