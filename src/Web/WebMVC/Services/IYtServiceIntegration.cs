using AllSub.Common.Models;
using AllSub.CommonCore.Interfaces.Http;
using System.Threading.Tasks;

namespace AllSub.WebMVC.Services
{
    public interface IYtServiceIntegration
    {
        Task<SearchCompletedEvent> FetchData(SearchRequestedEvent request);
    }
}
