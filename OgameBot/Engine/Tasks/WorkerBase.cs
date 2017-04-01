﻿using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Timers;

namespace OgameBot.Engine.Tasks
{
    public abstract class WorkerBase
    {
        private readonly Timer _timer;
        private bool _isRunning;
        public TimeSpan ExecutionInterval { get; set; }

        public WorkerBase()
        {
            ExecutionInterval = TimeSpan.FromMinutes(15);

            _timer = new Timer();
            _timer.Elapsed += TimerOnElapsed;
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            _timer.Stop();

            if (_isRunning)
            {
                // Run task
                RunInternal();
            }

            if (_isRunning)
            {
                // Restart timer
                _timer.Interval = ExecutionInterval.TotalMilliseconds;
                _timer.Start();
            }
        }

        protected abstract void RunInternal();

        public void Start()
        {
            _isRunning = true;
            Task.Factory.StartNew(() => TimerOnElapsed(null, null));
        }

        public void Stop()
        {
            _timer.Stop();
            _isRunning = false;
        }
    }
}