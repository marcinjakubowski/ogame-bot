using OgameBot.Objects.Types;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace OgameBot.Db.Parts
{
    [ComplexType]
    public class PlanetBuildings : Asset<BuildingType>
    {
        public PlanetBuildings() : base()
        {
        }
        protected PlanetBuildings(Dictionary<BuildingType, int> other) : base(other)
        {
        }

        public int? AllianceDepot
        {
            get { return TryGet(BuildingType.AllianceDepot); }
            set { TrySet(BuildingType.AllianceDepot, value); }
        }

        public int? CrystalMine
        {
            get { return TryGet(BuildingType.CrystalMine); }
            set { TrySet(BuildingType.CrystalMine, value); }
        }

        public int? CrystalStorage
        {
            get { return TryGet(BuildingType.CrystalStorage); }
            set { TrySet(BuildingType.CrystalStorage, value); }
        }

        public int? DeuteriumSynthesizer
        {
            get { return TryGet(BuildingType.DeuteriumSynthesizer); }
            set { TrySet(BuildingType.DeuteriumSynthesizer, value); }
        }

        public int? DeuteriumTank
        {
            get { return TryGet(BuildingType.DeuteriumTank); }
            set { TrySet(BuildingType.DeuteriumTank, value); }
        }

        public int? FusionReactor
        {
            get { return TryGet(BuildingType.FusionReactor); }
            set { TrySet(BuildingType.FusionReactor, value); }
        }

        public int? JumpGate
        {
            get { return TryGet(BuildingType.JumpGate); }
            set { TrySet(BuildingType.JumpGate, value); }
        }

        public int? LunarBase
        {
            get { return TryGet(BuildingType.LunarBase); }
            set { TrySet(BuildingType.LunarBase, value); }
        }

        public int? MetalMine
        {
            get { return TryGet(BuildingType.MetalMine); }
            set { TrySet(BuildingType.MetalMine, value); }
        }

        public int? MetalStorage
        {
            get { return TryGet(BuildingType.MetalStorage); }
            set { TrySet(BuildingType.MetalStorage, value); }
        }

        public int? MissileSilo
        {
            get { return TryGet(BuildingType.MissileSilo); }
            set { TrySet(BuildingType.MissileSilo, value); }
        }

        public int? NaniteFactory
        {
            get { return TryGet(BuildingType.NaniteFactory); }
            set { TrySet(BuildingType.NaniteFactory, value); }
        }

        public int? ResearchLab
        {
            get { return TryGet(BuildingType.ResearchLab); }
            set { TrySet(BuildingType.ResearchLab, value); }
        }

        public int? RoboticFactory
        {
            get { return TryGet(BuildingType.RoboticFactory); }
            set { TrySet(BuildingType.RoboticFactory, value); }
        }

        public int? SensorPhalanx
        {
            get { return TryGet(BuildingType.SensorPhalanx); }
            set { TrySet(BuildingType.SensorPhalanx, value); }
        }

        public int? Shipyard
        {
            get { return TryGet(BuildingType.Shipyard); }
            set { TrySet(BuildingType.Shipyard, value); }
        }

        public int? SolarPlant
        {
            get { return TryGet(BuildingType.SolarPlant); }
            set { TrySet(BuildingType.SolarPlant, value); }
        }

        public int? Terraformer
        {
            get { return TryGet(BuildingType.Terraformer); }
            set { TrySet(BuildingType.Terraformer, value); }
        }

        public static implicit operator PlanetBuildings(Dictionary<BuildingType, int> type)
        {
            return new PlanetBuildings(type);
        }

        public static implicit operator Dictionary<BuildingType, int>(PlanetBuildings type)
        {
            return type._dict;
        }
    }
}
