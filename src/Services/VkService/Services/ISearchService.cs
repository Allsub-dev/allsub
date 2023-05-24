using AllSub.Common.Models;

namespace AllSub.VkService.Services
{
    public interface ISearchService
    {
        Task<SearchCompletedEvent> FetchDataAsync(SearchRequestedEvent requestData);
    }
}