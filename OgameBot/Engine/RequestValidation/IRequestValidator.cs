using System.Net.Http;

namespace OgameBot.Engine.RequestValidation
{
    public interface IRequestValidator
    {
        HttpRequestMessage Validate(HttpRequestMessage request);
    }
}
