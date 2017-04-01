using System.Net.Http;
using OgameBot.Objects;
using System.Linq;
using OgameBot.Engine.Parsing.Objects;

namespace OgameBot.Engine.Commands
{
    public class ReadAllMessagesCommand : CommandBase
    {
        public ReadAllMessagesCommand(OGameClient client)
            : base(client)
        {
        }

        public override void Run()
        {
            // Read first page of all message types
            MessageTabType[] types = { MessageTabType.FleetsEspionage };

            foreach (MessageTabType type in types)
            {
                HttpRequestMessage req = Client.RequestBuilder.GetMessagePageRequest(type, 1);
                AssistedIssue(req);

                var page = ParsedObjects.OfType<MessagesPage>().FirstOrDefault();
                for (int pageNo=2; pageNo<=page.MaxPage; ++pageNo)
                {
                    HttpRequestMessage nextPage = Client.RequestBuilder.GetMessagePageRequest(type, pageNo);
                    AssistedIssue(nextPage);
                }
            }
        }
    }
}