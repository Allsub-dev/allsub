using AllSub.CommonCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllSub.Common.Models
{
    public record SearchRequestedEvent : IntegrationEvent
    {
        public string? ConnectionId { get; set; }
        public string? QueryString { get; set; }
        public bool OnlySubscriptions { get; set; }
        public int PageSize { get; set; } = 10;

        public UserData? UserPreferences { get; set; }
    }
}
