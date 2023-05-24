using AllSub.Common.Models;

namespace AllSub.TestAdService.Services
{
    public interface ISearchService
    {
        Task<SearchCompletedEvent> FetchAdsAsync(SearchRequestedEvent requestData);
    }
}