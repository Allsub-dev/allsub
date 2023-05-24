using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Microsoft.AspNetCore.Mvc;
using AllSub.Common.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AllSub.YtService.Services
{
    public class SearchService : ISearchService
    {
        private const string NEXTPAGETOKEN_KEY = "YtService.NextPageToken";
        private const string NEXTPAGETOKEN_NONE_VALUE = "YtService.None";

        private readonly ILogger<SearchService> _logger;
        private readonly string _defaultApiKey;

        public SearchService(ILogger<SearchService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _defaultApiKey = configuration["Secrets:Yt:DefaultApiKey"] ?? string.Empty;
            _logger.LogDebug("YtService.SearchService.DefaultApiKey: {_defaultApiKey}", _defaultApiKey);
        }

        public async Task<SearchCompletedEvent> FetchDataAsync(SearchRequestedEvent requestData)
        {
            _logger.LogDebug($"YtService.SearchService.FetchDataAsync called");
            string? email = null;
            if (requestData.UserPreferences != null)
            {
                // Do something if user authentificated
                email = requestData.UserPreferences.Email;
            }

            var items = new List<ServiceData>();
            var newPageToken = NEXTPAGETOKEN_NONE_VALUE;
            if (!string.IsNullOrWhiteSpace(requestData.QueryString) && !string.IsNullOrEmpty(_defaultApiKey))  // TODO: consider individual token
            {
                // Prepare request to actual API
                var youtubeService = new YouTubeService(new BaseClientService.Initializer()
                {
                    ApiKey = _defaultApiKey,
                    ApplicationName = "allsub.ru"
                });
                var searchListRequest = youtubeService.Search.List("snippet");
                searchListRequest.Q = requestData.QueryString;
                searchListRequest.MaxResults = requestData.PageSize;
                searchListRequest.Type = "video";
                searchListRequest.Order = SearchResource.ListRequest.OrderEnum.Relevance;

                if (requestData.DataDictionary.TryGetValue(NEXTPAGETOKEN_KEY, out string? nextPageToken))
                {
                    if (!string.IsNullOrWhiteSpace(nextPageToken) && 
                        !nextPageToken.Equals(NEXTPAGETOKEN_NONE_VALUE))
                    {
                        searchListRequest.PageToken = nextPageToken;
                    }
                }

                // Call the search.list method to retrieve results matching the specified query term.
                var searchListResponse = await searchListRequest.ExecuteAsync();
                
                if (!string.IsNullOrWhiteSpace(searchListResponse.NextPageToken))
                {
                    newPageToken = searchListResponse.NextPageToken;
                }
                
                List<string> videos = new List<string>();
                List<string> channels = new List<string>();
                List<string> playlists = new List<string>();
                // Add each result to the appropriate list, and then display the lists of
                // matching videos, channels, and playlists.
                foreach (var item in searchListResponse.Items)
                {
                    switch (item.Id.Kind)
                    {
                        case "youtube#video":
                            items.Add(new ServiceData
                            {
                                Id = item.Id.VideoId,
                                Type = ServiceType.YtService,
                                Url = $"https://www.youtube.com/watch?v={item.Id.VideoId}",
                                ImageUrl = item.Snippet.Thumbnails.Default__.Url,
                                Title = item.Snippet.Title,
                                Description = item.Snippet.Description,
                                Relevance = 5
                            });
                            break;
                        case "youtube#channel":
                            channels.Add(string.Format("{0} ({1})", item.Snippet.Title, item.Id.ChannelId));
                            break;
                        case "youtube#playlist":
                            playlists.Add(string.Format("{0} ({1})", item.Snippet.Title, item.Id.PlaylistId));
                            break;
                    }
                }
            }

            // Prepare response
            var ret = new SearchCompletedEvent()
            {
                ItemsAmount = int.MaxValue,    // TODO: Retrieve items amount from underlying service
                PageSize = requestData.PageSize,
                QueryString = requestData.QueryString,
                ServiceType = ServiceType.VkService,
                IsSuccesfull = true,
                ConnectionId = requestData.ConnectionId,
                Items = items.ToArray()
            };

            if (ret.DataDictionary.ContainsKey(NEXTPAGETOKEN_KEY))
            {
                ret.DataDictionary[NEXTPAGETOKEN_KEY] = newPageToken;
            }
            else
            {
                ret.DataDictionary.Add(NEXTPAGETOKEN_KEY, newPageToken);
            }

            return ret;
        }
    }
}
