﻿using System.Collections.Generic;
using System.Linq;
using OgameBot.Db;
using OgameBot.Engine.Parsing.Objects;
using OgameBot.Logging;
using ScraperClientLib.Engine.Parsing;
using System;
using OgameBot.Utilities;

namespace OgameBot.Engine.Savers
{
    public class EspionageReportSaver : SaverBase
    {
        public override void Run(List<DataObject> result)
        {
            List<EspionageReport> reports = result.OfType<EspionageReport>().ToList();
            if (!reports.Any())
                return;

            using (BotDb db = new BotDb())
            {
                long[] locIds = reports.Select(s => s.Coordinate.Id).ToArray();
                Dictionary<long, Planet> existing = db.Planets.Where(s => locIds.Contains(s.LocationId)).ToDictionary(s => s.LocationId);

                foreach (EspionageReport report in reports)
                {
                    Logger.Instance.Log(LogLevel.Info, $"Saving esp report on {report.Coordinate}, level: {report.Details}");

                    Planet item;
                    if (!existing.TryGetValue(report.Coordinate, out item))
                    {
                        item = new Planet
                        {
                            Coordinate = report.Coordinate
                        };

                        db.Planets.Add(item);
                    }

                    if (report.Details.HasFlag(ReportDetails.Resources) && (!item.LastResourcesTime.HasValue || report.Sent > item.LastResourcesTime))
                    {
                        item.Resources = report.Resources;
                        item.LastResourcesTime = report.Sent;
                    }

                    if (report.Details.HasFlag(ReportDetails.Buildings) && item.Buildings.NeedsUpdate(report.Sent))
                    {
                        item.Buildings = report.DetectedBuildings;
                        item.Buildings.LastUpdated = report.Sent;
                    }

                    if (report.Details.HasFlag(ReportDetails.Defense) && item.Defences.NeedsUpdate(report.Sent))
                    {
                        item.Defences = report.DetectedDefence;
                        item.Defences.LastUpdated = report.Sent;
                    }

                    if (report.Details.HasFlag(ReportDetails.Ships) && item.Ships.NeedsUpdate(report.Sent))
                    {
                        item.Ships = report.DetectedShips;
                        item.Ships.LastUpdated = report.Sent;
                    }

                    if (report.Details.HasFlag(ReportDetails.Research) && (item.Player?.Research.NeedsUpdate(report.Sent)).GetValueOrDefault(false))
                    {
                        item.Player.Research = report.DetectedResearch;
                        item.Player.Research.LastUpdated = report.Sent;
                    }

                    if (report.Details.HasFlag(ReportDetails.Ships) && report.DetectedShips != null)
                    {
                        if (!db.PlanetShipLog.Where(s => s.LocationId == item.Coordinate.Id && s.CreatedOn == report.Sent).Any())
                        {
                            PlanetShipLog log = new PlanetShipLog()
                            {
                                Planet = item,
                                Ships = report.DetectedShips,
                                CreatedOn = report.Sent
                            };
                            db.PlanetShipLog.Add(log);
                        }
                    }

                    var strippedTime = report.Sent.TruncateToMinute();

                    if (!db.PlanetActivityLog.Where(s => s.LocationId == item.Coordinate.Id && s.CreatedOn == strippedTime).Any())
                    {
                        PlanetActivityLog log = new PlanetActivityLog()
                        {
                            Planet = item,
                            Activity = report.Activity,
                            CreatedOn = strippedTime
                        };
                        db.PlanetActivityLog.Add(log);
                    }
                }

                db.SaveChanges();
            }
        }
    }
}