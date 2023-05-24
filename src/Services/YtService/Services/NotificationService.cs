using Microsoft.AspNetCore.SignalR;
using AllSub.Common.Models;
using AllSub.CommonCore.Interfaces.EventBus;
using AllSub.CommonCore.Models;

namespace AllSub.YtService.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ILogger _logger;
        private readonly IEventBus _eventBus;

        public NotificationService(ILogger<NotificationService> logger,
            IEventBus eventBus)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        }

        public void PublishSearchResult(SearchCompletedEvent searchData)
        {
            _logger.LogDebug("NotificationService PublishSearchResult started");
            var evt = (IntegrationEvent)searchData;
            try
            {
                _eventBus.Publish(evt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing search completed event: {IntegrationEventId} event: ({@IntegrationEvent})", evt.Id, evt);
            }
        }
    }
}
