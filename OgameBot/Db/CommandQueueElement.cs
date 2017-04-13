using System;
using OgameBot.Db.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OgameBot.Engine.Commands;
using OgameBot.Utilities;
using System.Text;

namespace OgameBot.Db
{
    [Table("CommandQueue")]
    public class CommandQueueElement : ILazySaver, ICreatedOn, IModifiedOn
    {
        private CommandBase _command;

        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(4096)]
        public string Parameters { get; set; }

        [Required]
        [MaxLength(128)]
        public string CommandType { get; set; }

        [NotMapped]
        public CommandBase Command
        {
            get
            {
                if (_command == null)
                {
                    Type type = Type.GetType(CommandType, true);
                    _command = (CommandBase)SerializerHelper.DeserializeFromString(type, Parameters);
                }

                return _command;
            }
            set { _command = value; }
        }
        public DateTimeOffset? ScheduledAt { get; set; }

        public DateTimeOffset CreatedOn { get; set; }
        public DateTimeOffset UpdatedOn { get; set; }

        public void Update()
        {
            Parameters = SerializerHelper.SerializeToString(Command);
            CommandType = Command.GetType().FullName;
        }

        public static implicit operator CommandBase(CommandQueueElement queueElement)
        {
            return queueElement.Command;
        }

        public static implicit operator CommandQueueElement(CommandBase cmd)
        {
            return new CommandQueueElement()
            {
                Command = cmd
            };
        }

    }
}
