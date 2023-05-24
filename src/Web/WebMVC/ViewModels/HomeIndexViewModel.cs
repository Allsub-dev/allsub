using AllSub.Common.Models;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace AllSub.WebMVC.ViewModels
{
    public class HomeIndexViewModel
    {
        public HomeIndexViewModel(IEnumerable<ServiceData>? commonAds) 
        { 
            CommonAds = commonAds ?? Enumerable.Empty<ServiceData>();
            WallItems = Enumerable.Empty<ServiceData>();
        }

        public IEnumerable<ServiceData> CommonAds { get; set; }
        public IEnumerable<ServiceData> WallItems { get; set; }
        public string? QueryString { get; set; }
    }
}
