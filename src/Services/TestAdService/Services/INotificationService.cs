using AllSub.Common.Models;

namespace AllSub.TestAdService.Services
{
    public interface INotificationService
    {
        void PublishSearchResult(SearchCompletedEvent searchData);
    }
}