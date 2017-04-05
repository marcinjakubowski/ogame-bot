﻿using OgameBot.Db;
using OgameBot.Engine.Parsing.Objects;
using OgameBot.Objects;
using System.Collections.Generic;
using System.Linq;

namespace OgameBot.Engine.Tasks.Farming.Strategies
{
    public interface IFarmingStrategy
    {
        void OnBeforeAttack(); // check how many ships, slots
        void OnAfterAttack(); // launch monitoring?

        IEnumerable<Planet> GetFarms(SystemCoordinate from, SystemCoordinate to); // query
        IEnumerable<AttackTarget> GetTargets(IEnumerable<EspionageReport> reports); // decide on the targets
    }
}