using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScraperClientLib.Engine.Parsing;
using OgameBot.Engine.Parsing.Objects;
using OgameBot.Objects.Types;
using OgameBot.Db;
using System.Net.Mail;
using System.Net;
using OgameBot.Logging;

namespace OgameBot.Engine.Savers
{
    public class HostileAttackEmailSender : SaverBase
    {
        private HashSet<int> ids = new HashSet<int>();
        private readonly string _fromAddress;
        private readonly string _toAddress;
        private readonly string _smtpServer;
        private readonly string _smtpLogin;
        private readonly string _smtpPassword;

        public HostileAttackEmailSender(string fromAddress, string toAddress, string smtpServer, string smtpLogin, string smtpPassword)
        {
            _fromAddress = fromAddress;
            _toAddress = toAddress;
            _smtpServer = smtpServer;
            _smtpLogin = smtpLogin;
            _smtpPassword = smtpPassword;
        }

        public override void Run(List<DataObject> result)
        {
            var hostileFleets = result.OfType<EventInfo>().Where(ev =>
                (ev.MissionType == MissionType.AcsAttack || ev.MissionType == MissionType.Attack) &&
                ev.Type == EventType.Hostile && 
                !ev.IsReturning
            );

            int fleetCount = hostileFleets.Count();
            if (fleetCount == 0) return;

            using (BotDb db = new BotDb())
            {
                var attacks = hostileFleets.Join(db.Planets, fleet => fleet.Destination.Coordinate.Id, planet => planet.Coordinate.Id, (fleet, planet) => new { Fleet = fleet, Planet = planet })
                                           .OrderBy(a => a.Fleet.ArrivalTime);

                string subject;
                StringBuilder body = new StringBuilder();

                int destinationCount = attacks.Select(a => a.Planet).Distinct().Count();

                var firstAttack = attacks.OrderBy(a => a.Fleet.ArrivalTime).FirstOrDefault();

                if (destinationCount == 1)
                {
                    subject = $"Attack on planet {firstAttack.Planet.Name} {firstAttack.Planet.Coordinate}, arriving at {firstAttack.Fleet.ArrivalTime}!";
                }
                else
                {
                    subject = $"Multiple attacks coming, first arriving at {firstAttack.Fleet.ArrivalTime}!";
                }

                foreach (var attack in attacks)
                {
                    if (ids.Contains(attack.Fleet.Id)) continue;

                    body.Append($"Attack on planet {attack.Planet.Name} {attack.Planet.Coordinate} from {attack.Fleet.Origin.EndpointName} {attack.Fleet.Origin.Coordinate} (player {attack.Fleet.Origin.Playername}), arriving at {attack.Fleet.ArrivalTime} - {attack.Fleet.Composition}.\n");
                    ids.Add(attack.Fleet.Id);
                }

                // Something new added
                if (body.Length > 0)
                {
                    Task.Factory.StartNew(() => SendEmail(subject, body.ToString()));
                }
                    

            }

        }

        private void SendEmail(string subject, string body)
        {
            var fromAddress = new MailAddress(_fromAddress, "OGameBot");
            var toAddress = new MailAddress(_toAddress, "OGamer");

            var smtp = new SmtpClient
            {
                Host = _smtpServer,
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Credentials = new NetworkCredential(_smtpLogin, _smtpPassword),
                Timeout = 20000,
            };

            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body
            })
            {
                smtp.Send(message);
            }
        }
    }
}
