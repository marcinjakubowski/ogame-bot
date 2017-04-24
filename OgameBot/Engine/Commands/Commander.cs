using OgameBot.Db;
using OgameBot.Engine.Tasks;
using OgameBot.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OgameBot.Engine.Commands
{
    public partial class CommandBase
    {

        public class Commander : WorkerBase
        {
            private object _lock = new object();

            public Commander()
            {
                ExecutionInterval = TimeSpan.FromSeconds(10);
            }

            protected override void RunInternal()
            {
                using (BotDb db = new BotDb())
                {
                    var scheduledCommands = db.CommandQueue.Where(q => q.ScheduledAt <= DateTimeOffset.Now).OrderBy(q => q.ScheduledAt).ToList();
                    foreach (var cmd in scheduledCommands)
                    {
                        Run(cmd, db);
                    }
                }
                
            }

            public void Run(CommandQueueElement next)
            {
                using (BotDb db = new BotDb())
                    Run(next, db);
            }

            private void Run(CommandQueueElement next, BotDb db)
            {
                lock (_lock)
                {
                    if (next.Id == 0)
                    {
                        db.CommandQueue.Add(next);
                        db.SaveChanges();
                    }
                    else
                    {
                        db.CommandQueue.Attach(next);
                    }
                    // Execute
                    if ((next.ScheduledAt ?? DateTimeOffset.Now) <= DateTimeOffset.Now)
                    {
                        Logger.Instance.Log(LogLevel.Debug, $"Running {next.Command} (id: {next.Id})");
                        CommandQueueElement following = next.Command.RunInternal();
                        next.ScheduledAt = null;

                        Logger.Instance.Log(LogLevel.Debug, $"Finished {next.Command} (id: {next.Id})");
                        if (following != null)
                        {
                            following.ScheduledBy = next;
                            db.CommandQueue.Add(following);
                            Logger.Instance.Log(LogLevel.Debug, $"Queueing {following.Command} (id: {following.Id}, by: {next.Id})");
                        }
                        db.SaveChanges();
                    }
                }
            }

            public void Run(CommandBase command, DateTimeOffset? at)
            {
                Run(new CommandQueueElement()
                {
                    Command = command,
                    ScheduledAt = at
                });
            }
        }
    }
}
