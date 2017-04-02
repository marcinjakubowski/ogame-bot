using OgameBot.Objects.Types;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace OgameBot.Db.Parts
{
    [ComplexType]
    public class PlanetDefences : Asset<DefenceType>
    {
        public PlanetDefences() : base()
        {
        }
        protected PlanetDefences(Dictionary<DefenceType, int> other) : base(other)
        {
        }

        public int? AntiBallisticMissiles
        {
            get { return TryGet(DefenceType.AntiBallisticMissiles); }
            set { TrySet(DefenceType.AntiBallisticMissiles, value); }
        }

        public int? GaussCannon
        {
            get { return TryGet(DefenceType.GaussCannon); }
            set { TrySet(DefenceType.GaussCannon, value); }
        }

        public int? HeavyLaser
        {
            get { return TryGet(DefenceType.HeavyLaser); }
            set { TrySet(DefenceType.HeavyLaser, value); }
        }

        public int? InterplanetaryMissiles
        {
            get { return TryGet(DefenceType.InterplanetaryMissiles); }
            set { TrySet(DefenceType.InterplanetaryMissiles, value); }
        }

        public int? IonCannon
        {
            get { return TryGet(DefenceType.IonCannon); }
            set { TrySet(DefenceType.IonCannon, value); }
        }

        public int? LargeShieldDome
        {
            get { return TryGet(DefenceType.LargeShieldDome); }
            set { TrySet(DefenceType.LargeShieldDome, value); }
        }

        public int? LightLaser
        {
            get { return TryGet(DefenceType.LightLaser); }
            set { TrySet(DefenceType.LightLaser, value); }
        }

        public int? PlasmaTurret
        {
            get { return TryGet(DefenceType.PlasmaTurret); }
            set { TrySet(DefenceType.PlasmaTurret, value); }
        }

        public int? RocketLauncher
        {
            get { return TryGet(DefenceType.RocketLauncher); }
            set { TrySet(DefenceType.RocketLauncher, value); }
        }

        public int? SmallShieldDome
        {
            get { return TryGet(DefenceType.SmallShieldDome); }
            set { TrySet(DefenceType.SmallShieldDome, value); }
        }

        public static implicit operator PlanetDefences(Dictionary<DefenceType, int> type)
        {
            return new PlanetDefences(type);
        }

        public static implicit operator Dictionary<DefenceType, int>(PlanetDefences type)
        {
            return type._dict;
        }
    }
}
