using System;
using OgameBot.Db.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;
using OgameBot.Db.Parts;
using System.ComponentModel.DataAnnotations;

namespace OgameBot.Db
{
    public class PlanetShipLog
    {
        [Key, Column(Order = 0)]
        public long LocationId { get; set; }
        [Key, Column(Order = 1)]
        public DateTimeOffset CreatedOn { get; set; }

        public PlanetShips Ships { get; set; }

        [ForeignKey(nameof(LocationId))]
        public virtual Planet Planet { get; set; }
    }
}
