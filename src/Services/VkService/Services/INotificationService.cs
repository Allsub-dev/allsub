using AllSub.Common.Models;

namespace AllSub.VkService.Services
{
    public interface INotificationService
    {
        void PublishSearchResult(SearchCompletedEvent searchData);
    }
}