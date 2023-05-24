using System.Collections.Generic;

namespace AllSub.WebMVC.Services
{
    public interface INotificationCache
    {
        void ClearData(string connectionId);

        IDictionary<string, string>? GetData(string connectionId);

        void SetData(string connectionId, IDictionary<string, string> dataDict);
    }
}
