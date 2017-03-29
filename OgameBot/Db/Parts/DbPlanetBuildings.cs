using OgameBot.Objects.Types;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace OgameBot.Db.Parts
{
    [ComplexType]
    public class DbPlanetBuildings
    {
        public int? AllianceDepot        { get; set; } = null;
        public int? CrystalMine          { get; set; } = null;
        public int? CrystalStorage       { get; set; } = null;
        public int? DeuteriumSynthesizer { get; set; } = null;
        public int? DeuteriumTank        { get; set; } = null;
        public int? FusionReactor        { get; set; } = null;
        public int? JumpGate             { get; set; } = null;
        public int? LunarBase            { get; set; } = null;
        public int? MetalMine            { get; set; } = null;
        public int? MetalStorage         { get; set; } = null;
        public int? MissileSilo          { get; set; } = null;
        public int? NaniteFactory        { get; set; } = null;
        public int? ResearchLab          { get; set; } = null;
        public int? RoboticFactory       { get; set; } = null;
        public int? SensorPhalanx        { get; set; } = null;
        public int? Shipyard             { get; set; } = null;
        public int? SolarPlant           { get; set; } = null;
        public int? Terraformer          { get; set; } = null;

        public static implicit operator DbPlanetBuildings(Dictionary<BuildingType, int> type)
        {
            return new DbPlanetBuildings()
            {
                AllianceDepot           = DbPlanet.GetFromDictionary(type, BuildingType.AllianceDepot       ),
                CrystalMine             = DbPlanet.GetFromDictionary(type, BuildingType.CrystalMine         ),
                CrystalStorage          = DbPlanet.GetFromDictionary(type, BuildingType.CrystalStorage      ),
                DeuteriumSynthesizer    = DbPlanet.GetFromDictionary(type, BuildingType.DeuteriumSynthesizer),
                DeuteriumTank           = DbPlanet.GetFromDictionary(type, BuildingType.DeuteriumTank       ),
                FusionReactor           = DbPlanet.GetFromDictionary(type, BuildingType.FusionReactor       ),
                JumpGate                = DbPlanet.GetFromDictionary(type, BuildingType.JumpGate            ),
                LunarBase               = DbPlanet.GetFromDictionary(type, BuildingType.LunarBase           ),
                MetalMine               = DbPlanet.GetFromDictionary(type, BuildingType.MetalMine           ),
                MetalStorage            = DbPlanet.GetFromDictionary(type, BuildingType.MetalStorage        ),
                MissileSilo             = DbPlanet.GetFromDictionary(type, BuildingType.MissileSilo         ),
                NaniteFactory           = DbPlanet.GetFromDictionary(type, BuildingType.NaniteFactory       ),
                ResearchLab             = DbPlanet.GetFromDictionary(type, BuildingType.ResearchLab         ),
                RoboticFactory          = DbPlanet.GetFromDictionary(type, BuildingType.RoboticFactory      ),
                SensorPhalanx           = DbPlanet.GetFromDictionary(type, BuildingType.SensorPhalanx       ),
                Shipyard                = DbPlanet.GetFromDictionary(type, BuildingType.Shipyard            ),
                SolarPlant              = DbPlanet.GetFromDictionary(type, BuildingType.SolarPlant          ),
                Terraformer             = DbPlanet.GetFromDictionary(type, BuildingType.Terraformer         )
            };
        }

        public static implicit operator Dictionary<BuildingType, int>(DbPlanetBuildings type)
        {
            var newType = new Dictionary<BuildingType, int>();
            if (type.AllianceDepot       .HasValue) newType[BuildingType.AllianceDepot       ] = (int)type.AllianceDepot       ;
            if (type.CrystalMine         .HasValue) newType[BuildingType.CrystalMine         ] = (int)type.CrystalMine         ;
            if (type.CrystalStorage      .HasValue) newType[BuildingType.CrystalStorage      ] = (int)type.CrystalStorage      ;
            if (type.DeuteriumSynthesizer.HasValue) newType[BuildingType.DeuteriumSynthesizer] = (int)type.DeuteriumSynthesizer;
            if (type.DeuteriumTank       .HasValue) newType[BuildingType.DeuteriumTank       ] = (int)type.DeuteriumTank       ;
            if (type.FusionReactor       .HasValue) newType[BuildingType.FusionReactor       ] = (int)type.FusionReactor       ;
            if (type.JumpGate            .HasValue) newType[BuildingType.JumpGate            ] = (int)type.JumpGate            ;
            if (type.LunarBase           .HasValue) newType[BuildingType.LunarBase           ] = (int)type.LunarBase           ;
            if (type.MetalMine           .HasValue) newType[BuildingType.MetalMine           ] = (int)type.MetalMine           ;
            if (type.MetalStorage        .HasValue) newType[BuildingType.MetalStorage        ] = (int)type.MetalStorage        ;
            if (type.MissileSilo         .HasValue) newType[BuildingType.MissileSilo         ] = (int)type.MissileSilo         ;
            if (type.NaniteFactory       .HasValue) newType[BuildingType.NaniteFactory       ] = (int)type.NaniteFactory       ;
            if (type.ResearchLab         .HasValue) newType[BuildingType.ResearchLab         ] = (int)type.ResearchLab         ;
            if (type.RoboticFactory      .HasValue) newType[BuildingType.RoboticFactory      ] = (int)type.RoboticFactory      ;
            if (type.SensorPhalanx       .HasValue) newType[BuildingType.SensorPhalanx       ] = (int)type.SensorPhalanx       ;
            if (type.Shipyard            .HasValue) newType[BuildingType.Shipyard            ] = (int)type.Shipyard            ;
            if (type.SolarPlant          .HasValue) newType[BuildingType.SolarPlant          ] = (int)type.SolarPlant          ;
            if (type.Terraformer         .HasValue) newType[BuildingType.Terraformer         ] = (int)type.Terraformer         ;

            return newType;
        }
    }
}
