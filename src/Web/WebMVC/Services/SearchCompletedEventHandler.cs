using Microsoft.Extensions.Logging;
using AllSub.Common.Models;
using AllSub.CommonCore.Interfaces.EventBus;
using Serilog.Context;
using System.Threading.Tasks;

namespace AllSub.WebMVC.Services
{
    public class SearchCompletedEventHandler : IIntegrationEventHandler<SearchCompletedEvent>
    {
        private readonly ILogger<SearchCompletedEventHandler> _logger;
        private readonly INotificationService _notificationService;

        public SearchCompletedEventHandler(ILogger<SearchCompletedEventHandler> logger, 
            INotificationService notificationService)
        {
            _logger = logger;
            _notificationService = notificationService;
        }

        public async Task Handle(SearchCompletedEvent @event)
        {
            _logger.LogDebug("----- Handling SearchCompletedEvent: eventid={IntegrationEventId}; ConnectionId: {AppName} - ({@IntegrationEvent})", 
                @event.Id, 
                @event.ConnectionId, 
                @event);

            await _notificationService.SearchCompletedAsync(@event);
        }
    }
}
