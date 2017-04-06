using OgameBot.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OgameBot.Engine.Commands
{
    public class RecallFleetCommand : CommandBase
    {
        public int FleetId { get; set; }

        public RecallFleetCommand(OGameClient client, int fleetId) : base(client)
        {
            FleetId = fleetId;
        }

        public override void Run()
        {
            var req = Client.RequestBuilder.PostPage(Objects.Types.PageType.FleetMovement, new [] 
            {
                KeyValuePair.Create("return", FleetId.ToString())
            });

            Client.IssueRequest(req);
        }
    }
}
