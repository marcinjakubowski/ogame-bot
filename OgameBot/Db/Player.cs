﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OgameBot.Db.Interfaces;
using OgameBot.Engine.Parsing.Objects;
using OgameBot.Db.Parts;

namespace OgameBot.Db
{
    public class Player : ICreatedOn, IModifiedOn
    {
        public Player()
        {
            Research = new PlayerResearch();
        }

        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int PlayerId { get; set; }

        [MaxLength(128)]
        public string Name { get; set; }

        public int Ranking { get; set; }

        public PlayerStatus Status { get; set; }
        public PlayerResearch Research { get; set; }

        public DateTimeOffset CreatedOn { get; set; }
        public DateTimeOffset UpdatedOn { get; set; }

        public virtual ICollection<Planet> Planets { get; set; }

        public override string ToString()
        {
            return $"{PlayerId} ({Name})";
        }
    }
}