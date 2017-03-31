using OgameBot.Objects.Types;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace OgameBot.Db.Parts
{
    [ComplexType]
    public class PlanetBuildings
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

        public void FromPartialResult(Dictionary<BuildingType, int> dict)
        {
            if (dict.ContainsKey(BuildingType.AllianceDepot       )) AllianceDepot        = dict[BuildingType.AllianceDepot       ];
            if (dict.ContainsKey(BuildingType.CrystalMine         )) CrystalMine          = dict[BuildingType.CrystalMine         ];
            if (dict.ContainsKey(BuildingType.CrystalStorage      )) CrystalStorage       = dict[BuildingType.CrystalStorage      ];
            if (dict.ContainsKey(BuildingType.DeuteriumSynthesizer)) DeuteriumSynthesizer = dict[BuildingType.DeuteriumSynthesizer];
            if (dict.ContainsKey(BuildingType.DeuteriumTank       )) DeuteriumTank        = dict[BuildingType.DeuteriumTank       ];
            if (dict.ContainsKey(BuildingType.FusionReactor       )) FusionReactor        = dict[BuildingType.FusionReactor       ];
            if (dict.ContainsKey(BuildingType.JumpGate            )) JumpGate             = dict[BuildingType.JumpGate            ];
            if (dict.ContainsKey(BuildingType.LunarBase           )) LunarBase            = dict[BuildingType.LunarBase           ];
            if (dict.ContainsKey(BuildingType.MetalMine           )) MetalMine            = dict[BuildingType.MetalMine           ];
            if (dict.ContainsKey(BuildingType.MetalStorage        )) MetalStorage         = dict[BuildingType.MetalStorage        ];
            if (dict.ContainsKey(BuildingType.MissileSilo         )) MissileSilo          = dict[BuildingType.MissileSilo         ];
            if (dict.ContainsKey(BuildingType.NaniteFactory       )) NaniteFactory        = dict[BuildingType.NaniteFactory       ];
            if (dict.ContainsKey(BuildingType.ResearchLab         )) ResearchLab          = dict[BuildingType.ResearchLab         ];
            if (dict.ContainsKey(BuildingType.RoboticFactory      )) RoboticFactory       = dict[BuildingType.RoboticFactory      ];
            if (dict.ContainsKey(BuildingType.SensorPhalanx       )) SensorPhalanx        = dict[BuildingType.SensorPhalanx       ];
            if (dict.ContainsKey(BuildingType.Shipyard            )) Shipyard             = dict[BuildingType.Shipyard            ];
            if (dict.ContainsKey(BuildingType.SolarPlant          )) SolarPlant           = dict[BuildingType.SolarPlant          ];
            if (dict.ContainsKey(BuildingType.Terraformer         )) Terraformer          = dict[BuildingType.Terraformer         ];
        }

        public static implicit operator PlanetBuildings(Dictionary<BuildingType, int> type)
        {
            return new PlanetBuildings()
            {
                AllianceDepot           = Planet.GetFromDictionary(type, BuildingType.AllianceDepot       ),
                CrystalMine             = Planet.GetFromDictionary(type, BuildingType.CrystalMine         ),
                CrystalStorage          = Planet.GetFromDictionary(type, BuildingType.CrystalStorage      ),
                DeuteriumSynthesizer    = Planet.GetFromDictionary(type, BuildingType.DeuteriumSynthesizer),
                DeuteriumTank           = Planet.GetFromDictionary(type, BuildingType.DeuteriumTank       ),
                FusionReactor           = Planet.GetFromDictionary(type, BuildingType.FusionReactor       ),
                JumpGate                = Planet.GetFromDictionary(type, BuildingType.JumpGate            ),
                LunarBase               = Planet.GetFromDictionary(type, BuildingType.LunarBase           ),
                MetalMine               = Planet.GetFromDictionary(type, BuildingType.MetalMine           ),
                MetalStorage            = Planet.GetFromDictionary(type, BuildingType.MetalStorage        ),
                MissileSilo             = Planet.GetFromDictionary(type, BuildingType.MissileSilo         ),
                NaniteFactory           = Planet.GetFromDictionary(type, BuildingType.NaniteFactory       ),
                ResearchLab             = Planet.GetFromDictionary(type, BuildingType.ResearchLab         ),
                RoboticFactory          = Planet.GetFromDictionary(type, BuildingType.RoboticFactory      ),
                SensorPhalanx           = Planet.GetFromDictionary(type, BuildingType.SensorPhalanx       ),
                Shipyard                = Planet.GetFromDictionary(type, BuildingType.Shipyard            ),
                SolarPlant              = Planet.GetFromDictionary(type, BuildingType.SolarPlant          ),
                Terraformer             = Planet.GetFromDictionary(type, BuildingType.Terraformer         )
            };
        }

        public static implicit operator Dictionary<BuildingType, int>(PlanetBuildings type)
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
