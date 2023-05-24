namespace AllSub.CommonCore.Interfaces.Http
{
    public interface ICallerResult<T> : IHttpCallerResult
    {
        T? Result { get; }
    }
}
