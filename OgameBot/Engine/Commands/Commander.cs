﻿using OgameBot.Db;
using OgameBot.Engine.Tasks;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OgameBot.Engine.Commands
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
                    if ((next.ScheduledAt ?? DateTimeOffset.Now) < DateTimeOffset.Now)
                    {
                        CommandQueueElement following = next.Command.Run();
                        
                        next.ScheduledAt = null;

                        if (following != null)
                        {
                            following.ScheduledBy = next;
                            db.CommandQueue.Add(following);
                        }
                        db.SaveChanges();
                        rerun = true;
                    }
                    else
                    {
                        if (_next == null || _next.ScheduledAt > next.ScheduledAt)
                            _next = next;

                        ExecutionInterval = (_next.ScheduledAt.Value - DateTimeOffset.Now).Add(TimeSpan.FromSeconds(1));
                    }
                }


                if (rerun) RunInternal();
            }

        }

        public void Run(CommandBase command, DateTimeOffset? at = null)
        {
            Run(new CommandQueueElement()
            {
                Command = command,
                ScheduledAt = at
            });
        }

        public async Task RunAsync(CommandBase op)
        {
            await Task.Factory.StartNew(() => Run(op));
        }
    }
}
