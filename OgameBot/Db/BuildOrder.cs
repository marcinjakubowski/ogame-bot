using OgameBot.Db.Interfaces;
using OgameBot.Objects.Types;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OgameBot.Db
{
    public class BuildOrder : ICreatedOn, IModifiedOn
    {
        [Key]
        public int Id { get; set; }

        public long LocationId { get; set; }
        public BuildingType Building { get; set; }
        public int Level { get; set; }
        public DateTimeOffset? CompletedAt { get; set; }

        [ForeignKey(nameof(LocationId))]
        public virtual Planet Planet { get; set; }

        public DateTimeOffset CreatedOn { get; set; }
        public DateTimeOffset UpdatedOn { get; set; }
        
    }
}
