using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using AllSub.Common.Models;
using AllSub.WebMVC.Models;
using AllSub.WebMVC.Services;
using AllSub.WebMVC.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace AllSub.WebMVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ITestAdServiceIntegration _testAdService;
        private readonly IYtServiceIntegration _ytServiceIntegration;
        private readonly IVkServiceIntegration _vkService;
        private readonly IServiceProvider _serviceProvider;

        public HomeController(ILogger<HomeController> logger,
            IServiceProvider serviceProvider,
            ITestAdServiceIntegration testAdService,
            IYtServiceIntegration ytServiceIntegration,
            IVkServiceIntegration vkService)
        {
            _logger = logger;
            _testAdService = testAdService;
            _vkService = vkService;
            _ytServiceIntegration = ytServiceIntegration;
            _serviceProvider = serviceProvider;
        }

        public async Task<IActionResult> Index()
        {
            var serviceRequest = new SearchRequestedEvent
            {
                PageSize = 5,
                QueryString = string.Empty,
                UserPreferences = null
            };

            var userName = User?.Identity?.Name;
            if (!string.IsNullOrWhiteSpace(userName))
            {
                serviceRequest.UserPreferences = new UserData { Email = userName };

                using (var scope = _serviceProvider.CreateScope())
                {
                    var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
                    var adminUser = await userMgr.FindByNameAsync(userName);
                }
            }

            var tasks = new List<Task<SearchCompletedEvent>>
            {
                _testAdService.FetchAds(serviceRequest),
            };

            SearchCompletedEvent[] completedTasks = await Task.WhenAll(tasks);   // TODO: consider to move try/catch from services to the controller

            return View(BuildViewModel(completedTasks.Where(t => t.IsSuccesfull), string.Empty)); // TODO: process errors
        }

        private const int AdInsertDist = 3;

        private HomeIndexViewModel BuildViewModel(IEnumerable<SearchCompletedEvent> completedTasks, string? searchString)
        {
            var ads = completedTasks.Where(r => r.ServiceType == ServiceType.AdService).SelectMany(t => t.Items).ToArray();
            var viewModel = new HomeIndexViewModel(ads.Take(3));

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                viewModel.QueryString = searchString;
                var dataList = completedTasks
                    .Where(r => r.ServiceType != ServiceType.AdService)
                    .SelectMany(t => t.Items)
                    .OrderByDescending(d => d.Relevance).ToList();

                var insertPoint = AdInsertDist;
                foreach (var addData in ads.Skip(3))
                {
                    if (insertPoint < dataList.Count)
                    {
                        dataList.Insert(insertPoint, addData);
                        insertPoint++;
                    }
                    insertPoint += AdInsertDist;
                }
                viewModel.WallItems = dataList;
                _logger.LogInformation("HomeController gets {count} items from the services.", dataList.Count);
            }

            return viewModel;
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}