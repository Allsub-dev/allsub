using AllSub.CommonCore.Interfaces.Http;
using System.Net;

namespace AllSub.Common.Http
{
    internal class HttpCallerResult<T> : ICallerResult<T>
    {
        public HttpCallerResult(HttpResponseMessage response, T? result)
        {
            StatusCode = response.StatusCode;
            Result = result;
            IsSuccessCode = response.IsSuccessStatusCode;
            ReasonPhrase = response.ReasonPhrase;
        }

        public HttpStatusCode StatusCode { get; }

        public T? Result { get; }

        public bool IsSuccessCode { get; }

        public string? ReasonPhrase { get; }
    }
}
