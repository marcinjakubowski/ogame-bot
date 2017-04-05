﻿using System;
using System.Collections.Generic;
using System.Linq;
using OgameBot.Engine.Parsing.Objects;
using OgameBot.Db;
using OgameBot.Objects;
using OgameBot.Objects.Types;
using OgameBot.Logging;

namespace OgameBot.Engine.Tasks.Farming.Strategies
{
    public class InactiveFarmingStrategy : IFarmingStrategy
    {
        public int MinimumRanking { get; set; } = 99999;
        public int MinimumTotalStorageLevel { get; set; } = 5;
        public int SlotsLeaveRemaining { get; set; } = 1;
        public int MinimumCargosToSend { get; set; } = 2;
        public Resources ResourcePriority { get; set; } = new Resources(1, 1, 1);

        private OGameClient _client;
        private ShipType _cargo;

        private int _cargoCount = 0;
        private FleetSlotCount _slots;

        public InactiveFarmingStrategy(OGameClient client) : this(client, ShipType.LargeCargo)
        {
        }

        public InactiveFarmingStrategy(OGameClient client, ShipType cargo)
        {
            _client = client;
            _cargo = cargo;
        }


        public IEnumerable<Planet> GetFarms(SystemCoordinate from, SystemCoordinate to)
        {
            using (BotDb db = new BotDb())
            {
                var farms = db.Planets.Where(s =>
                        s.LocationId >= from.LowerCoordinate && s.LocationId <= to.UpperCoordinate
                        && (s.Player.Status.HasFlag(PlayerStatus.Inactive) || s.Player.Status.HasFlag(PlayerStatus.LongInactive))
                        && !s.Player.Status.HasFlag(PlayerStatus.Vacation)
                        && !s.Player.Status.HasFlag(PlayerStatus.Admin)
                        && s.Player.Ranking < MinimumRanking
                        && (s.Buildings.LastUpdated == null || s.Buildings.MetalStorage + s.Buildings.CrystalStorage + s.Buildings.DeuteriumTank > MinimumTotalStorageLevel)
                ).ToList();

                return farms;
            }
                
        }

        public IEnumerable<AttackTarget> GetTargets(IEnumerable<EspionageReport> reports)
        {
            var farmsToAttack = reports.Where(m =>
                                   m.Details.HasFlag(ReportDetails.Defense) && m.DetectedDefence == null &&
                                   m.Details.HasFlag(ReportDetails.Ships) && m.DetectedShips == null)
                            .OrderByDescending(m => m.Resources.TotalWithPriority(ResourcePriority));

            foreach (EspionageReport farm in farmsToAttack)
            {
                if (!AreSlotsAvailable() || !AreCargosAvailable()) yield break;


                AttackTarget target = new AttackTarget()
                {
                    Destination = farm.Coordinate,
                    Fleet = FleetComposition.ToPlunder(farm.Resources, _cargo),
                    ExpectedPlunder = FleetComposition.GetPlunder(farm.Resources)
                };

                int ships = target.Fleet.Ships[_cargo];
                if (ships < MinimumCargosToSend) yield break;

                _cargoCount -= ships;
                _slots.Current++;

                yield return target;
            }
        }

        

        public void OnAfterAttack()
        {
        }

        public void OnBeforeAttack()
        {
            var resp = _client.IssueRequest(_client.RequestBuilder.GetPage(PageType.Fleet));

            int? cargoCount = resp.GetParsed<DetectedShip>().Where(s => s.Ship == ShipType.LargeCargo).FirstOrDefault()?.Count;
            _cargoCount = cargoCount.HasValue ? (int)cargoCount : 0;
            if (!AreCargosAvailable())
            {
                Logger.Instance.Log(LogLevel.Error, "There are no cargos on the planet");
            }

            _slots = resp.GetParsedSingle<FleetSlotCount>();
            if (!AreSlotsAvailable())
            {
                Logger.Instance.Log(LogLevel.Error, "There are no fleet slots available");
            }
        }

        private bool AreSlotsAvailable()
        {
            return (_slots.Max - _slots.Current > SlotsLeaveRemaining);
        }

        private bool AreCargosAvailable()
        {
            return _cargoCount > 0;
        }
    }
}