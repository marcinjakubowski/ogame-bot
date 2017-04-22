using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OgameBot.Db
{
    public abstract class PlanetLog
    {
        [Key, Column(Order = 0)]
        public long LocationId { get; set; }
        [Key, Column(Order = 1)]
        public DateTimeOffset CreatedOn { get; set; }

        [ForeignKey(nameof(LocationId))]
        public virtual Planet Planet { get; set; }
    }
}
