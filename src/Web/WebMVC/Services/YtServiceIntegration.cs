using Microsoft.Extensions.Logging;
using AllSub.Common.Http;
using AllSub.Common.Models;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace AllSub.WebMVC.Services
{
    internal class YtServiceIntegration : BaseHttpCaller, IYtServiceIntegration
    {
        private readonly ILogger _logger;
        protected override ILogger Log => _logger;

        public YtServiceIntegration(HttpClient httpClient, ILogger<YtServiceIntegration> logger) : base(httpClient)
        {
            _logger = logger;
            string baseUrl = "http://yt-service";    // TODO: get from settings

            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                throw new ArgumentNullException(nameof(baseUrl));
            }
            if (!Uri.IsWellFormedUriString(baseUrl, UriKind.Absolute))
            {
                throw new ArgumentException($"{baseUrl} is not a wellformed absolute uri");
            }

            httpClient.BaseAddress = new Uri(baseUrl, UriKind.Absolute);
            _logger.LogDebug("YtServiceIntegration instance created, base address: '{baseUrl}'", baseUrl);
        }

        public async Task<SearchCompletedEvent> FetchData(SearchRequestedEvent request)
        {
            try
            {
                var response = await PostAsync<SearchRequestedEvent, SearchCompletedEvent>("YtService/FetchData", request).ConfigureAwait(false);
                if (response != null && response.IsSuccessCode && response.Result != null)
                {
                    return response.Result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching data from path 'YtService/FetchData'");
            }

            return new SearchCompletedEvent
            {
                ServiceType = ServiceType.YtService,
                QueryString = request.QueryString,
                PageSize = request.PageSize,
                ItemsAmount = 0,
                Items = Array.Empty<ServiceData>(),
                IsSuccesfull = false
            };
        }
    }
}
