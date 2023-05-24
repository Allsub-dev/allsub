using AllSub.Common.Models;

namespace AllSub.YtService.Services
{
    public interface ISearchService
    {
        Task<SearchCompletedEvent> FetchDataAsync(SearchRequestedEvent requestData);
    }
}