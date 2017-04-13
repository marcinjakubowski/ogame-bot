using OgameBot.Db;
using OgameBot.Engine.Parsing.Objects;
using OgameBot.Objects;
using System.Collections.Generic;

namespace OgameBot.Engine.Commands.Farming.Strategies
{
    public interface IFarmingStrategy
    {
        bool OnBeforeAttack(); // check how many ships, slots
        void OnAfterAttack(); // launch monitoring?

        IEnumerable<Planet> GetFarms(SystemCoordinate from, SystemCoordinate to); // query
        IEnumerable<Target> GetTargets(IEnumerable<EspionageReport> reports); // decide on the targets

        int ProbeCount { get; set; }
    }
}
