using System.Net;

namespace AllSub.CommonCore.Interfaces.Http
{
    public interface IHttpCallerResult
    {
        HttpStatusCode StatusCode { get; }
        string? ReasonPhrase { get; }
        bool IsSuccessCode { get; }
    }
}
