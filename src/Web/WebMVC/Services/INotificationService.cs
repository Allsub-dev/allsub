using AllSub.Common.Models;
using System;
using System.Threading.Tasks;

namespace AllSub.WebMVC.Services
{
    public interface INotificationService
    {
        void StartSearch(SearchRequestedEvent request);

        Task SearchCompletedAsync(SearchCompletedEvent eventData);

        void ClearConnectionData(string connectionId);
    }
}