using Microsoft.AspNetCore.Mvc;
using AllSub.Common.Models;
using Serilog;
using System.Net;
using VkNet.Model.RequestParams;
using VkNet.Model;
using VkNet;
using AllSub.VkService.Services;

namespace AllSub.VkService.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class VkServiceController : ControllerBase
    {
        private readonly ILogger<VkServiceController> _logger;
        private readonly ISearchService _searchService;


        public VkServiceController(ILogger<VkServiceController> logger, ISearchService searchService)
        {
            _logger = logger;
            _searchService = searchService;
        }

        [HttpPost(Name = "FetchData")]
        [ProducesResponseType(typeof(SearchCompletedEvent), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<SearchCompletedEvent>> FetchDataAsync([FromBody] SearchRequestedEvent requestData)
        {
            _logger.LogDebug($"VkService.TestServiceController.FetchDataAsync called");
            return Ok(await _searchService.FetchDataAsync(requestData));
        }
    }
}