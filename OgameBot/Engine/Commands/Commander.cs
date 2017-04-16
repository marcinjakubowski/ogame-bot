using OgameBot.Db;
using OgameBot.Engine.Tasks;
using OgameBot.Logging;
using System;
using System.Linq;

namespace OgameBot.Engine.Commands
{
    public partial class CommandBase
    {

        public class Commander : WorkerBase
        {
            private object _lock = new object();
            CommandQueueElement _next;

            public Commander()
            {
                ExecutionInterval = TimeSpan.FromSeconds(10);
            }

            protected override void RunInternal()
            {
                CommandQueueElement next = null;
                using (BotDb db = new BotDb())
                {
                    next = db.CommandQueue.Where(q => q.ScheduledAt != null).OrderBy(q => q.ScheduledAt).FirstOrDefault();
                    if (next == null) return;
                }
                Run(next);

            }

            public void Run(CommandQueueElement next)
            {
                lock (_lock)
                {
                    bool rerun = false;
                    using (BotDb db = new BotDb())
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
                            rerun = true;
                        }
                        else
                        {
                            if (_next == null || _next.ScheduledAt > next.ScheduledAt)
                                _next = next;

                            ExecutionInterval = (_next.ScheduledAt.Value - DateTimeOffset.Now).Add(TimeSpan.FromSeconds(1));
                            if (ExecutionInterval.TotalSeconds < 0) ExecutionInterval = TimeSpan.FromSeconds(10);
                        }
                    }


                    if (rerun) RunInternal();
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
