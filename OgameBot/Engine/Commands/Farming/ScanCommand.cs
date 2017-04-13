using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using OgameBot.Db;
using OgameBot.Objects;
using OgameBot.Logging;

namespace OgameBot.Engine.Commands.Farming
{
    public class ScanCommand : CommandBase
    {
        private static readonly TimeSpan RescanInterval = TimeSpan.FromHours(6);

        public SystemCoordinate From { get; set; }
        public SystemCoordinate To { get; set; }

        public ScanCommand()
        {
            if (To < From)
            {
                var tmp = From;
                From = To;
                To = tmp;
            }
        }

        public override void Run()
        {
            // Get existing scan infoes
            Dictionary<int, GalaxyScan> existing;
            using (BotDb db = new BotDb())
            {
                existing = db.Scans.Where(s => From.Id <= s.LocationId && s.LocationId <= To.Id).ToDictionary(s => s.LocationId);
            }

            int count = (To.Galaxy - From.Galaxy + 1) * (To.System - From.System + 1);

            Logger.Instance.Log(LogLevel.Info, $"Scanning between {From} and {To}: {count} systems");
            for (byte gal = From.Galaxy; gal <= To.Galaxy; gal++)
            {
                short sFrom = gal == From.Galaxy ? From.System : (short)0;
                short sTo = gal == To.Galaxy ? To.System : Client.Settings.Systems;

                for (short sys = sFrom; sys <= sTo; sys++)
                {
                    SystemCoordinate coord = new SystemCoordinate(gal, sys);

                    GalaxyScan exists;
                    if (existing.TryGetValue(coord, out exists))
                    {
                        if (DateTimeOffset.Now - exists.LastScan < RescanInterval)
                            // Ignore
                            continue;
                    }

                    // Scan
                    HttpRequestMessage req = Client.RequestBuilder.GetGalaxyContent(coord);
                    Client.IssueRequest(req);

                    if (--count % 10 == 0 && count > 0)
                    {
                        Logger.Instance.Log(LogLevel.Info, $"{count} systems remaining to scan...");
                    }
                }
            }
        }

    }
}
