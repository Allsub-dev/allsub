using Microsoft.AspNetCore.Mvc;
using AllSub.Common.Models;
using VkNet.Model.RequestParams;
using VkNet.Model;
using VkNet;
using Microsoft.AspNetCore.DataProtection.KeyManagement;

namespace AllSub.VkService.Services
{
    public class SearchService : ISearchService
    {
        private const string PAGENUM_KEY = "VkService.PageNum";

        private readonly ILogger<SearchService> _logger;
        private readonly string _defaultAccessToken;

        public SearchService(ILogger<SearchService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _defaultAccessToken = configuration["Secrets:Vk:DefaultAccessToken"] ?? string.Empty;
            _logger.LogDebug("VkService.SearchService._defaultAccessToken: {_defaultAccessToken}", _defaultAccessToken);
        }

        public async Task<SearchCompletedEvent> FetchDataAsync(SearchRequestedEvent requestData)
        {
            _logger.LogDebug($"VkService.SearchService.FetchDataAsync called");
            string? email = null;
            if (requestData.UserPreferences != null)
            {
                // Do something if user authentificated
                email = requestData.UserPreferences.Email;
            }

            var items = new List<ServiceData>();
            int pageNum = 1;
            if (!string.IsNullOrWhiteSpace(requestData.QueryString) && !string.IsNullOrEmpty(_defaultAccessToken))  // TODO: consider individual token
            {
                // Prepare request to actual API
                var api = new VkApi();

                api.Authorize(new ApiAuthParams
                {
                    AccessToken = _defaultAccessToken
                });

                var offset = 0;
                if (requestData.DataDictionary.TryGetValue(PAGENUM_KEY, out string? strPageNum))
                {
                    if (!int.TryParse(strPageNum, out pageNum))
                    {
                        pageNum = 1;
                    }
                }

                if (pageNum > 1)
                {
                    offset = (pageNum - 1) * requestData.PageSize;
                }

                var req = new VideoSearchParams()
                {
                    Adult = true,
                    Count = requestData.PageSize,
                    Offset = offset,
                    Sort = VkNet.Enums.VideoSort.Relevance,
                    Query = requestData.QueryString
                };


                VkNet.Utils.VkCollection<VkNet.Model.Attachments.Video> res = await api.Video.SearchAsync(req);
                foreach (var v in res)
                {
                    var data = new ServiceData
                    {
                        Id = v.Id.ToString(),
                        Type = ServiceType.VkService,
                        Url = v.Player.AbsoluteUri,
                        ImageUrl = v.Image?.FirstOrDefault()?.Url.AbsoluteUri,
                        Title = v.Title,
                        Description = v.Description,
                        Relevance = 5
                    };
                    items.Add(data);
                }
            }

            // Prepare response
            var ret = new SearchCompletedEvent()
            {
                PageSize = requestData.PageSize,
                QueryString = requestData.QueryString,
                ServiceType = ServiceType.VkService,
                IsSuccesfull = true,
                ConnectionId = requestData.ConnectionId,
                Items = items.ToArray()
            };

            var newPageNum = (++pageNum).ToString();
            if (ret.DataDictionary.ContainsKey(PAGENUM_KEY))
            {
                ret.DataDictionary[PAGENUM_KEY] = newPageNum;
            }
            else
            {
                ret.DataDictionary.Add(PAGENUM_KEY, newPageNum);
            }

            return ret;
        }
    }
}
