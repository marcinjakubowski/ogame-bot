using OgameBot.Db;
using OgameBot.Engine.Parsing.Objects;
using OgameBot.Objects;
using OgameBot.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace OgameBot.Engine.Commands
{
    public class ReadAllMessagesCommand : CommandBase
    {
        public override CommandQueueElement Run()
        {
            // Read all messages
            FindAllMessagesCommand cmd = new FindAllMessagesCommand();
            cmd.Run();

            List<MessagesPage> messagePages = cmd.ParsedObjects.OfType<MessagesPage>().ToList();
            List<int> messageIds = messagePages.SelectMany(s => s.MessageIds).Select(s => s.Item1).ToList();

            HashSet<int> existing;
            using (BotDb db = new BotDb())
                existing = db.Messages.Where(s => messageIds.Contains(s.MessageId)).Select(s => s.MessageId).ToHashset();

            foreach (MessagesPage messagesPage in messagePages)
            {
                // Request each message
                foreach (Tuple<int, MessageType> message in messagesPage.MessageIds)
                {
                    if (existing.Contains(message.Item1))
                        // Already fetched
                        continue;

                    if (message.Item2 == MessageType.EspionageReport)
                    {
                        HttpRequestMessage req = Client.RequestBuilder.GetMessagePage(message.Item1, MessageTabType.FleetsEspionage);
                        AssistedIssue(req);
                    }
                }
            }
            return null;
        }
    }
}
