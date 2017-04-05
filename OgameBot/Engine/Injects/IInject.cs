using ScraperClientLib.Engine;

namespace OgameBot.Engine.Injects
{
    public interface IInject
    {
        string Inject(string body, ResponseContainer repsonse);
    }
}
