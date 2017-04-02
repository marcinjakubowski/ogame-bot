using OgameBot.Objects.Types;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace OgameBot.Db.Parts
{
    [ComplexType]
    public class PlayerResearch : Asset<ResearchType>
    {
        public PlayerResearch() : base()
        {
        }
        protected PlayerResearch(Dictionary<ResearchType, int> other) : base(other)
        {
        }

        public int? ArmourTechnology
        {
            get { return TryGet(ResearchType.ArmourTechnology); }
            set { TrySet(ResearchType.ArmourTechnology, value);  }
        }

        public int? Astrophysics
        {
            get { return TryGet(ResearchType.Astrophysics); }
            set { TrySet(ResearchType.Astrophysics, value); }
        }

        public int? CombustionDrive
        {
            get { return TryGet(ResearchType.CombustionDrive); }
            set { TrySet(ResearchType.CombustionDrive, value); }
        }

        public int? ComputerTechnology
        {
            get { return TryGet(ResearchType.ComputerTechnology); }
            set { TrySet(ResearchType.ComputerTechnology, value); }
        }

        public int? EnergyTechnology
        {
            get { return TryGet(ResearchType.EnergyTechnology); }
            set { TrySet(ResearchType.EnergyTechnology, value); }
        }

        public int? EspionageTechnology
        {
            get { return TryGet(ResearchType.EspionageTechnology); }
            set { TrySet(ResearchType.EspionageTechnology, value); }
        }

        public int? GravitonTechnology
        {
            get { return TryGet(ResearchType.GravitonTechnology); }
            set { TrySet(ResearchType.GravitonTechnology, value); }
        }

        public int? HyperspaceDrive
        {
            get { return TryGet(ResearchType.HyperspaceDrive); }
            set { TrySet(ResearchType.HyperspaceDrive, value); }
        }

        public int? HyperspaceTechnology
        {
            get { return TryGet(ResearchType.HyperspaceTechnology); }
            set { TrySet(ResearchType.HyperspaceTechnology, value); }
        }

        public int? ImpulseDrive
        {
            get { return TryGet(ResearchType.ImpulseDrive); }
            set { TrySet(ResearchType.ImpulseDrive, value); }
        }

        public int? IntergalacticResearchNetwork
        {
            get { return TryGet(ResearchType.IntergalacticResearchNetwork); }
            set { TrySet(ResearchType.IntergalacticResearchNetwork, value); }
        }

        public int? IonTechnology
        {
            get { return TryGet(ResearchType.IonTechnology); }
            set { TrySet(ResearchType.IonTechnology, value); }
        }

        public int? LaserTechnology
        {
            get { return TryGet(ResearchType.LaserTechnology); }
            set { TrySet(ResearchType.LaserTechnology, value); }
        }

        public int? PlasmaTechnology
        {
            get { return TryGet(ResearchType.PlasmaTechnology); }
            set { TrySet(ResearchType.PlasmaTechnology, value); }
        }

        public int? ShieldingTechnology
        {
            get { return TryGet(ResearchType.ShieldingTechnology); }
            set { TrySet(ResearchType.ShieldingTechnology, value); }
        }

        public int? WeaponsTechnology
        {
            get { return TryGet(ResearchType.WeaponsTechnology); }
            set { TrySet(ResearchType.WeaponsTechnology, value); }
        }

        public static implicit operator PlayerResearch(Dictionary<ResearchType, int> type)
        {
            return new PlayerResearch(type);
        }

        public static implicit operator Dictionary<ResearchType, int>(PlayerResearch type)
        {
            return type._dict;
        }
    }
}
