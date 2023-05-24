using AllSub.Common.Models;

namespace AllSub.YtService.Services
{
    public interface INotificationService
    {
        void PublishSearchResult(SearchCompletedEvent searchData);
    }
}