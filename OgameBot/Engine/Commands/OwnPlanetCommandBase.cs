using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OgameBot.Engine.Commands
{
    public abstract class OwnPlanetCommandBase : CommandBase
    {
        public OwnPlanetCommandBase(OGameClient client) : base(client)
        {

        }
    }
}
