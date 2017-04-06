using OgameBot.Engine.Parsing.Objects;
using ScraperClientLib.Engine;

namespace OgameBot.Engine.Injects
{
    public interface IInject
    {
        string Inject(OgamePageInfo info, string body, ResponseContainer response);
    }
}
