using System.Collections.Generic;
using System.Linq;
using OgameBot.Db;
using OgameBot.Engine.Parsing.Objects;
using OgameBot.Logging;
using ScraperClientLib.Engine.Parsing;

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

                    if (report.Details.HasFlag(ReportDetails.Resources) && report.Sent > item.LastResourcesTime)
                    {
                        item.Resources = report.Resources;
                        item.LastResourcesTime = report.Sent;
                    }

                    if (report.Details.HasFlag(ReportDetails.Buildings) && report.Sent > item.LastBuildingsTime)
                    {
                        item.Buildings = report.DetectedBuildings;
                        item.LastBuildingsTime = report.Sent;
                    }

                    if (report.Details.HasFlag(ReportDetails.Defense) && report.Sent > item.LastDefencesTime)
                    {
                        item.Defences = report.DetectedDefence;
                        item.LastDefencesTime = report.Sent;
                    }

                    if (report.Details.HasFlag(ReportDetails.Ships) && report.Sent > item.LastShipsTime)
                    {
                        item.Ships = report.DetectedShips;
                        item.LastShipsTime = report.Sent;
                    }

                    if (report.Details.HasFlag(ReportDetails.Research) && report.Sent > item.Player.LastResearchTime)
                    {
                        item.Player.Research = report.DetectedResearch;
                        item.Player.LastResearchTime = report.Sent;
                    }
                }

                db.SaveChanges();
            }
        }
    }
}