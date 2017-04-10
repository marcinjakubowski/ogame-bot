using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OgameBot.Engine
{
    public interface IPlanetExclusiveOperation
    {
        int PlanetId { get; }
        string Name { get; }
        string Progress { get; }
    }
}
