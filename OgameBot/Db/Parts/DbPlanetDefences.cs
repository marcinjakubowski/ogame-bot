using OgameBot.Objects.Types;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace OgameBot.Db.Parts
{
    [ComplexType]
    public class DbPlanetDefences
    {
        public int? AntiBallisticMissiles  { get; set; } = null;
        public int? GaussCannon            { get; set; } = null;
        public int? HeavyLaser             { get; set; } = null;
        public int? InterplanetaryMissiles { get; set; } = null;
        public int? IonCannon              { get; set; } = null;
        public int? LargeShieldDome        { get; set; } = null;
        public int? LightLaser             { get; set; } = null;
        public int? PlasmaTurret           { get; set; } = null;
        public int? RocketLauncher         { get; set; } = null;
        public int? SmallShieldDome        { get; set; } = null;

        public static implicit operator DbPlanetDefences(Dictionary<DefenceType, int> type)
        {
            return new DbPlanetDefences()
            {
                AntiBallisticMissiles  = DbPlanet.GetFromDictionary(type, DefenceType.AntiBallisticMissiles ),
                GaussCannon            = DbPlanet.GetFromDictionary(type, DefenceType.GaussCannon           ),
                HeavyLaser             = DbPlanet.GetFromDictionary(type, DefenceType.HeavyLaser            ),
                InterplanetaryMissiles = DbPlanet.GetFromDictionary(type, DefenceType.InterplanetaryMissiles),
                IonCannon              = DbPlanet.GetFromDictionary(type, DefenceType.IonCannon             ),
                LargeShieldDome        = DbPlanet.GetFromDictionary(type, DefenceType.LargeShieldDome       ),
                LightLaser             = DbPlanet.GetFromDictionary(type, DefenceType.LightLaser            ),
                PlasmaTurret           = DbPlanet.GetFromDictionary(type, DefenceType.PlasmaTurret          ),
                RocketLauncher         = DbPlanet.GetFromDictionary(type, DefenceType.RocketLauncher        ),
                SmallShieldDome        = DbPlanet.GetFromDictionary(type, DefenceType.SmallShieldDome       )
            };
        }

        public static implicit operator Dictionary<DefenceType, int>(DbPlanetDefences type)
        {
            var newType = new Dictionary<DefenceType, int>();
            if (type.AntiBallisticMissiles .HasValue) newType[DefenceType.AntiBallisticMissiles ] = (int)type.AntiBallisticMissiles ;
            if (type.GaussCannon           .HasValue) newType[DefenceType.GaussCannon           ] = (int)type.GaussCannon           ;
            if (type.HeavyLaser            .HasValue) newType[DefenceType.HeavyLaser            ] = (int)type.HeavyLaser            ;
            if (type.InterplanetaryMissiles.HasValue) newType[DefenceType.InterplanetaryMissiles] = (int)type.InterplanetaryMissiles;
            if (type.IonCannon             .HasValue) newType[DefenceType.IonCannon             ] = (int)type.IonCannon             ;
            if (type.LargeShieldDome       .HasValue) newType[DefenceType.LargeShieldDome       ] = (int)type.LargeShieldDome       ;
            if (type.LightLaser            .HasValue) newType[DefenceType.LightLaser            ] = (int)type.LightLaser            ;
            if (type.PlasmaTurret          .HasValue) newType[DefenceType.PlasmaTurret          ] = (int)type.PlasmaTurret          ;
            if (type.RocketLauncher        .HasValue) newType[DefenceType.RocketLauncher        ] = (int)type.RocketLauncher        ;
            if (type.SmallShieldDome       .HasValue) newType[DefenceType.SmallShieldDome       ] = (int)type.SmallShieldDome       ;

            return newType;
        }
    }
}
