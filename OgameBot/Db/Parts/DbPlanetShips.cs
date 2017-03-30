﻿using OgameBot.Objects.Types;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace OgameBot.Db.Parts
{
    [ComplexType]
    public class DbPlanetShips
    {
        public int? Battlecruiser  { get; set; } = null;
        public int? Battleship     { get; set; } = null;
        public int? Bomber         { get; set; } = null;
        public int? ColonyShip     { get; set; } = null;
        public int? Cruiser        { get; set; } = null;
        public int? Deathstar      { get; set; } = null;
        public int? Destroyer      { get; set; } = null;
        public int? EspionageProbe { get; set; } = null;
        public int? HeavyFighter   { get; set; } = null;
        public int? LargeCargo     { get; set; } = null;
        public int? LightFighter   { get; set; } = null;
        public int? Recycler       { get; set; } = null;
        public int? SmallCargo     { get; set; } = null;
        public int? SolarSatellite { get; set; } = null;

        public void FromPartialResult(Dictionary<ShipType, int> dict)
        {
            if (dict.ContainsKey(ShipType.Battlecruiser )) Battlecruiser  = dict[ShipType.Battlecruiser ];
            if (dict.ContainsKey(ShipType.Battleship    )) Battleship     = dict[ShipType.Battleship    ];
            if (dict.ContainsKey(ShipType.Bomber        )) Bomber         = dict[ShipType.Bomber        ];
            if (dict.ContainsKey(ShipType.ColonyShip    )) ColonyShip     = dict[ShipType.ColonyShip    ];
            if (dict.ContainsKey(ShipType.Cruiser       )) Cruiser        = dict[ShipType.Cruiser       ];
            if (dict.ContainsKey(ShipType.Deathstar     )) Deathstar      = dict[ShipType.Deathstar     ];
            if (dict.ContainsKey(ShipType.Destroyer     )) Destroyer      = dict[ShipType.Destroyer     ];
            if (dict.ContainsKey(ShipType.EspionageProbe)) EspionageProbe = dict[ShipType.EspionageProbe];
            if (dict.ContainsKey(ShipType.HeavyFighter  )) HeavyFighter   = dict[ShipType.HeavyFighter  ];
            if (dict.ContainsKey(ShipType.LargeCargo    )) LargeCargo     = dict[ShipType.LargeCargo    ];
            if (dict.ContainsKey(ShipType.LightFighter  )) LightFighter   = dict[ShipType.LightFighter  ];
            if (dict.ContainsKey(ShipType.Recycler      )) Recycler       = dict[ShipType.Recycler      ];
            if (dict.ContainsKey(ShipType.SmallCargo    )) SmallCargo     = dict[ShipType.SmallCargo    ];
            if (dict.ContainsKey(ShipType.SolarSatellite)) SolarSatellite = dict[ShipType.SolarSatellite];
        }

        public static implicit operator DbPlanetShips(Dictionary<ShipType, int> type)
        {
            return new DbPlanetShips()
            {
                Battlecruiser  = DbPlanet.GetFromDictionary(type, ShipType.Battlecruiser ),
                Battleship     = DbPlanet.GetFromDictionary(type, ShipType.Battleship    ),
                Bomber         = DbPlanet.GetFromDictionary(type, ShipType.Bomber        ),
                ColonyShip     = DbPlanet.GetFromDictionary(type, ShipType.ColonyShip    ),
                Cruiser        = DbPlanet.GetFromDictionary(type, ShipType.Cruiser       ),
                Deathstar      = DbPlanet.GetFromDictionary(type, ShipType.Deathstar     ),
                Destroyer      = DbPlanet.GetFromDictionary(type, ShipType.Destroyer     ),
                EspionageProbe = DbPlanet.GetFromDictionary(type, ShipType.EspionageProbe),
                HeavyFighter   = DbPlanet.GetFromDictionary(type, ShipType.HeavyFighter  ),
                LargeCargo     = DbPlanet.GetFromDictionary(type, ShipType.LargeCargo    ),
                LightFighter   = DbPlanet.GetFromDictionary(type, ShipType.LightFighter  ),
                Recycler       = DbPlanet.GetFromDictionary(type, ShipType.Recycler      ),
                SmallCargo     = DbPlanet.GetFromDictionary(type, ShipType.SmallCargo    ),
                SolarSatellite = DbPlanet.GetFromDictionary(type, ShipType.SolarSatellite)
            };
        }

        public static implicit operator Dictionary<ShipType, int>(DbPlanetShips type)
        {
            var newType = new Dictionary<ShipType, int>();
            if (type.Battlecruiser .HasValue) newType[ShipType.Battlecruiser ] = (int)type.Battlecruiser ;
            if (type.Battleship    .HasValue) newType[ShipType.Battleship    ] = (int)type.Battleship    ;
            if (type.Bomber        .HasValue) newType[ShipType.Bomber        ] = (int)type.Bomber        ;
            if (type.ColonyShip    .HasValue) newType[ShipType.ColonyShip    ] = (int)type.ColonyShip    ;
            if (type.Cruiser       .HasValue) newType[ShipType.Cruiser       ] = (int)type.Cruiser       ;
            if (type.Deathstar     .HasValue) newType[ShipType.Deathstar     ] = (int)type.Deathstar     ;
            if (type.Destroyer     .HasValue) newType[ShipType.Destroyer     ] = (int)type.Destroyer     ;
            if (type.EspionageProbe.HasValue) newType[ShipType.EspionageProbe] = (int)type.EspionageProbe;
            if (type.HeavyFighter  .HasValue) newType[ShipType.HeavyFighter  ] = (int)type.HeavyFighter  ;
            if (type.LargeCargo    .HasValue) newType[ShipType.LargeCargo    ] = (int)type.LargeCargo    ;
            if (type.LightFighter  .HasValue) newType[ShipType.LightFighter  ] = (int)type.LightFighter  ;
            if (type.Recycler      .HasValue) newType[ShipType.Recycler      ] = (int)type.Recycler      ;
            if (type.SmallCargo    .HasValue) newType[ShipType.SmallCargo    ] = (int)type.SmallCargo    ;
            if (type.SolarSatellite.HasValue) newType[ShipType.SolarSatellite] = (int)type.SolarSatellite;

            return newType;
        }
    }
}
