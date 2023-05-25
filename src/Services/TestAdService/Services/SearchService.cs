using AllSub.Common.Models;

namespace AllSub.TestAdService.Services
{
    public class SearchService : ISearchService
    {
        private readonly ILogger<SearchService> _logger;

        public SearchService(ILogger<SearchService> logger)
        {
            _logger = logger;
        }

        public async Task<SearchCompletedEvent> FetchAdsAsync(SearchRequestedEvent requestData)
        {
            _logger.LogDebug($"TestService.SearchService.FetchDataAsync called");
            string? email = null;
            if (requestData.UserPreferences != null)
            {
                // Do something if user authentificated
                email = requestData.UserPreferences.Email;
            }

            // Prepare request to actual API

            // Prepare response
            var ret = new SearchCompletedEvent()
            {
                ItemsAmount = int.MaxValue,    // TODO: Retrieve items amount from underlying service
                // TODO: PageNumber = requestData.PageNumber,
                PageSize = requestData.PageSize,
                QueryString = requestData.QueryString,
                ServiceType = ServiceType.AdService,
                IsSuccesfull = true,
                ConnectionId = requestData.ConnectionId
            };
            var items = new List<ServiceData>();
            var images = new string[] {
                "https://www.mssdevlab.com/img/birthdays.png",
                "https://www.mssdevlab.com/img/zoom.png",
                "https://www.mssdevlab.com/img/visa.png"
            };
            var curImage = 0;
            for (int i = 0; i < ret.PageSize; i++)
            {
                var data = new ServiceData
                {
                    Id = i.ToString(),
                    Type = ServiceType.AdService,
                    Url = "https://www.mssdevlab.com",
                    ImageUrl = images[curImage++],
                    Title = $"TestAdService index:{i + 1}",
                    Description = $"Example of the advertisment from provider. TestAdService email:'{email}' query:'{requestData.QueryString}'",
                    Relevance = i
                };
                items.Add(data);
                if (curImage > 2)
                {
                    curImage = 0;
                }
            }
            ret.Items = items.ToArray();

            return await Task.FromResult(ret);
        }
    }
}
