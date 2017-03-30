using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OgameBot.Db.Interfaces;
using OgameBot.Db.Parts;
using OgameBot.Objects;
using OgameBot.Utilities;
using System.Collections.Generic;

namespace OgameBot.Db
{
    public class DbPlanet : ICreatedOn, IModifiedOn
    {
        public DbPlanet()
        {
            Resources = new DbResources();
            Buildings = new DbPlanetBuildings();
            Defences = new DbPlanetDefences();
            Ships = new DbPlanetShips();
        }

        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long LocationId { get; set; }

        // cp
        public int? PlanetId { get; set; }

        public Coordinate Coordinate
        {
            get { return CoordHelper.GetCoordinate(LocationId); }
            set { LocationId = CoordHelper.ToNumber(value); }
        }

        public DateTimeOffset CreatedOn { get; set; }

        public DateTimeOffset UpdatedOn { get; set; }


        public DbResources Resources { get; set; }
        public DbPlanetBuildings Buildings { get; set; }
        public DbPlanetShips Ships { get; set; }
        public DbPlanetDefences Defences { get; set; }

        public DateTimeOffset LastResourcesTime { get; set; }

        public DateTimeOffset LastBuildingsTime { get; set; }

        public DateTimeOffset LastShipsTime { get; set; }

        public DateTimeOffset LastDefencesTime { get; set; }

        [MaxLength(255)]
        public string Name { get; set; }

        public int? PlayerId { get; set; }

        [ForeignKey(nameof(PlayerId))]
        public virtual DbPlayer Player { get; set; }

        public override string ToString()
        {
            return $"DbPlanet {Coordinate}, id: {LocationId}";
        }

        public static int? GetFromDictionary<T>(Dictionary<T, int> dict, T type)
        {
            if (dict == null || !dict.ContainsKey(type)) return 0;

            return dict[type];
        }
    }
}