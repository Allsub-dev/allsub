using AllSub.Common.Models;
using System.Threading.Tasks;

namespace AllSub.WebMVC.Services
{
    public interface ITestAdServiceIntegration
    {
        Task<SearchCompletedEvent> FetchAds(SearchRequestedEvent request);
    }
}
