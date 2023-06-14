using Elasticsearch.Net;
using Microsoft.AspNetCore.SignalR;
using AllSub.Common.Models;
using AllSub.WebMVC.Services;
using System;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AllSub.WebMVC.Hubs
{
    public class SearchHub : Hub
    {
        private readonly INotificationService _notificationService;
        public SearchHub(INotificationService notificationService) 
        { 
            _notificationService = notificationService;
        }

        public void Search(string searchString, bool onlySubscriptions)
        {
            var serviceRequest = new SearchRequestedEvent
            {
                OnlySubscriptions = onlySubscriptions,
                PageSize = 10,
                QueryString = searchString ?? string.Empty,
                ConnectionId = Context?.ConnectionId
            };

            var userName = Context?.User?.Identity?.Name;
            if (!string.IsNullOrWhiteSpace(userName))
            {
                serviceRequest.UserPreferences = new UserData { Email = userName };
            }

            _notificationService.StartSearch(serviceRequest);
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            _notificationService.ClearConnectionData(Context.ConnectionId);

            return base.OnDisconnectedAsync(exception);
        }
    }
}
