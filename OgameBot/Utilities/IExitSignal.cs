using System;

namespace OgameBot.Utilities
{
    // http://stackoverflow.com/questions/6546509/detect-when-console-application-is-closing-killed
    public interface IExitSignal
    {
        event EventHandler Exit;
    }
}
