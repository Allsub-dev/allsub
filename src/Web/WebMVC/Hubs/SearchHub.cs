using Elasticsearch.Net;
using Microsoft.AspNetCore.SignalR;
using AllSub.Common.Models;
using AllSub.WebMVC.Services;
using System;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using static System.Runtime.InteropServices.JavaScript.JSType;
using AllSub.WebMVC.Data;
using Microsoft.AspNetCore.Identity;
using System.Linq;

namespace AllSub.WebMVC.Hubs
{
    public class SearchHub : Hub
    {
        private readonly INotificationService _notificationService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _userDbContext;

        public SearchHub(INotificationService notificationService, 
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext userDbContext) 
        { 
            _notificationService = notificationService;
            _userManager = userManager;
            _userDbContext = userDbContext;
        }

        public async Task Search(string searchString, bool onlySubscriptions)
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
                var user = await _userManager.FindByNameAsync(userName);
                if (user != null)
                {
                    await _userDbContext.Entry(user).Collection(x => x.UserProperties).LoadAsync();
                    serviceRequest.UserPreferences = new UserData
                    {
                        Email = userName,
                        Properties = user.UserProperties.ToList()
                    };
                }
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
