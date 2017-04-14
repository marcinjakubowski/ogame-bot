﻿using System;
using OgameBot.Db;
using OgameBot.Proxy;
using System.Collections.Specialized;

namespace OgameBot.Engine.Commands
{
    public class RunProxyCommand : CommandBase
    {
        public string Command { get; set; }
        public NameValueCollection Parameters { get; set; }

        public override CommandQueueElement Run()
        {
            OgameClientProxy.Instance.RunCommand(Command, Parameters);
            return null;
        }
    }
}
