using OgameBot.Objects.Types;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace OgameBot.Db.Parts
{
    [ComplexType]
    public class PlayerResearch
    {
        public int? ArmourTechnology             { get; set; } = null;
        public int? Astrophysics                 { get; set; } = null;
        public int? CombustionDrive              { get; set; } = null;
        public int? ComputerTechnology           { get; set; } = null;
        public int? EnergyTechnology             { get; set; } = null;
        public int? EspionageTechnology          { get; set; } = null;
        public int? GravitonTechnology           { get; set; } = null;
        public int? HyperspaceDrive              { get; set; } = null;
        public int? HyperspaceTechnology         { get; set; } = null;
        public int? ImpulseDrive                 { get; set; } = null;
        public int? IntergalacticResearchNetwork { get; set; } = null;
        public int? IonTechnology                { get; set; } = null;
        public int? LaserTechnology              { get; set; } = null;
        public int? PlasmaTechnology             { get; set; } = null;
        public int? ShieldingTechnology          { get; set; } = null;
        public int? WeaponsTechnology            { get; set; } = null;

        public void FromPartialResult(Dictionary<ResearchType, int> dict)
        {
            if (dict.ContainsKey(ResearchType.ArmourTechnology            )) ArmourTechnology             = dict[ResearchType.ArmourTechnology            ];
            if (dict.ContainsKey(ResearchType.Astrophysics                )) Astrophysics                 = dict[ResearchType.Astrophysics                ];
            if (dict.ContainsKey(ResearchType.CombustionDrive             )) CombustionDrive              = dict[ResearchType.CombustionDrive             ];
            if (dict.ContainsKey(ResearchType.ComputerTechnology          )) ComputerTechnology           = dict[ResearchType.ComputerTechnology          ];
            if (dict.ContainsKey(ResearchType.EnergyTechnology            )) EnergyTechnology             = dict[ResearchType.EnergyTechnology            ];
            if (dict.ContainsKey(ResearchType.EspionageTechnology         )) EspionageTechnology          = dict[ResearchType.EspionageTechnology         ];
            if (dict.ContainsKey(ResearchType.GravitonTechnology          )) GravitonTechnology           = dict[ResearchType.GravitonTechnology          ];
            if (dict.ContainsKey(ResearchType.HyperspaceDrive             )) HyperspaceDrive              = dict[ResearchType.HyperspaceDrive             ];
            if (dict.ContainsKey(ResearchType.HyperspaceTechnology        )) HyperspaceTechnology         = dict[ResearchType.HyperspaceTechnology        ];
            if (dict.ContainsKey(ResearchType.ImpulseDrive                )) ImpulseDrive                 = dict[ResearchType.ImpulseDrive                ];
            if (dict.ContainsKey(ResearchType.IntergalacticResearchNetwork)) IntergalacticResearchNetwork = dict[ResearchType.IntergalacticResearchNetwork];
            if (dict.ContainsKey(ResearchType.IonTechnology               )) IonTechnology                = dict[ResearchType.IonTechnology               ];
            if (dict.ContainsKey(ResearchType.LaserTechnology             )) LaserTechnology              = dict[ResearchType.LaserTechnology             ];
            if (dict.ContainsKey(ResearchType.PlasmaTechnology            )) PlasmaTechnology             = dict[ResearchType.PlasmaTechnology            ];
            if (dict.ContainsKey(ResearchType.ShieldingTechnology         )) ShieldingTechnology          = dict[ResearchType.ShieldingTechnology         ];
            if (dict.ContainsKey(ResearchType.WeaponsTechnology           )) WeaponsTechnology            = dict[ResearchType.WeaponsTechnology           ];
        }

        public static implicit operator PlayerResearch(Dictionary<ResearchType, int> type)
        {
            return new PlayerResearch()
            {
                ArmourTechnology             = Planet.GetFromDictionary(type, ResearchType.ArmourTechnology            ),
                Astrophysics                 = Planet.GetFromDictionary(type, ResearchType.Astrophysics                ),
                CombustionDrive              = Planet.GetFromDictionary(type, ResearchType.CombustionDrive             ),
                ComputerTechnology           = Planet.GetFromDictionary(type, ResearchType.ComputerTechnology          ),
                EnergyTechnology             = Planet.GetFromDictionary(type, ResearchType.EnergyTechnology            ),
                EspionageTechnology          = Planet.GetFromDictionary(type, ResearchType.EspionageTechnology         ),
                GravitonTechnology           = Planet.GetFromDictionary(type, ResearchType.GravitonTechnology          ),
                HyperspaceDrive              = Planet.GetFromDictionary(type, ResearchType.HyperspaceDrive             ),
                HyperspaceTechnology         = Planet.GetFromDictionary(type, ResearchType.HyperspaceTechnology        ),
                ImpulseDrive                 = Planet.GetFromDictionary(type, ResearchType.ImpulseDrive                ),
                IntergalacticResearchNetwork = Planet.GetFromDictionary(type, ResearchType.IntergalacticResearchNetwork),
                IonTechnology                = Planet.GetFromDictionary(type, ResearchType.IonTechnology               ),
                LaserTechnology              = Planet.GetFromDictionary(type, ResearchType.LaserTechnology             ),
                PlasmaTechnology             = Planet.GetFromDictionary(type, ResearchType.PlasmaTechnology            ),
                ShieldingTechnology          = Planet.GetFromDictionary(type, ResearchType.ShieldingTechnology         ),
                WeaponsTechnology            = Planet.GetFromDictionary(type, ResearchType.WeaponsTechnology           )
            };
        }

        public static implicit operator Dictionary<ResearchType, int>(PlayerResearch type)
        {
            var newType = new Dictionary<ResearchType, int>();

            if (type.ArmourTechnology            .HasValue) newType[ResearchType.ArmourTechnology            ] = (int)type.ArmourTechnology            ;
            if (type.Astrophysics                .HasValue) newType[ResearchType.Astrophysics                ] = (int)type.Astrophysics                ; 
            if (type.CombustionDrive             .HasValue) newType[ResearchType.CombustionDrive             ] = (int)type.CombustionDrive             ; 
            if (type.ComputerTechnology          .HasValue) newType[ResearchType.ComputerTechnology          ] = (int)type.ComputerTechnology          ; 
            if (type.EnergyTechnology            .HasValue) newType[ResearchType.EnergyTechnology            ] = (int)type.EnergyTechnology            ;
            if (type.EspionageTechnology         .HasValue) newType[ResearchType.EspionageTechnology         ] = (int)type.EspionageTechnology         ;
            if (type.GravitonTechnology          .HasValue) newType[ResearchType.GravitonTechnology          ] = (int)type.GravitonTechnology          ;
            if (type.HyperspaceDrive             .HasValue) newType[ResearchType.HyperspaceDrive             ] = (int)type.HyperspaceDrive             ;
            if (type.HyperspaceTechnology        .HasValue) newType[ResearchType.HyperspaceTechnology        ] = (int)type.HyperspaceTechnology        ;
            if (type.ImpulseDrive                .HasValue) newType[ResearchType.ImpulseDrive                ] = (int)type.ImpulseDrive                ;
            if (type.IntergalacticResearchNetwork.HasValue) newType[ResearchType.IntergalacticResearchNetwork] = (int)type.IntergalacticResearchNetwork;
            if (type.IonTechnology               .HasValue) newType[ResearchType.IonTechnology               ] = (int)type.IonTechnology               ;
            if (type.LaserTechnology             .HasValue) newType[ResearchType.LaserTechnology             ] = (int)type.LaserTechnology             ;
            if (type.PlasmaTechnology            .HasValue) newType[ResearchType.PlasmaTechnology            ] = (int)type.PlasmaTechnology            ;
            if (type.ShieldingTechnology         .HasValue) newType[ResearchType.ShieldingTechnology         ] = (int)type.ShieldingTechnology         ;
            if (type.WeaponsTechnology           .HasValue) newType[ResearchType.WeaponsTechnology           ] = (int)type.WeaponsTechnology           ;

            return newType;
        }
    }
}
