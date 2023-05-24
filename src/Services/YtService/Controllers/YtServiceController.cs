using Microsoft.AspNetCore.Mvc;
using AllSub.Common.Models;
using AllSub.YtService.Services;
using Serilog;
using System.Net;

namespace AllSub.TestService.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class YtServiceController : ControllerBase
    {
        private readonly ILogger<YtServiceController> _logger;
        private readonly ISearchService _searchService;

        public YtServiceController(ILogger<YtServiceController> logger, 
            ISearchService searchService)
        {
            _logger = logger;
            _searchService = searchService;
        }

        [HttpPost(Name = "FetchData")]
        [ProducesResponseType(typeof(SearchCompletedEvent), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<SearchCompletedEvent>> FetchDataAsync([FromBody] SearchRequestedEvent requestData)
        {
            _logger.LogDebug($"YtService.YtServiceController.FetchDataAsync called");
            return Ok(await _searchService.FetchDataAsync(requestData));
        }
    }
}