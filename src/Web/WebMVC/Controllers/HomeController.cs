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
using AllSub.WebMVC.Data;

namespace AllSub.WebMVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View(BuildViewModel(string.Empty)); // TODO: process errors
        }

        private HomeIndexViewModel BuildViewModel(string? searchString)
        {
            var viewModel = new HomeIndexViewModel(Enumerable.Empty<ServiceData>());

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                viewModel.QueryString = searchString;
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