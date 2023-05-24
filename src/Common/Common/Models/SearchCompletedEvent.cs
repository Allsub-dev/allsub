using AllSub.CommonCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace AllSub.Common.Models
{
    public record SearchCompletedEvent : IntegrationEvent
    {
        public SearchCompletedEvent() 
        { 
            Items = Array.Empty<ServiceData>();
        }
        public ServiceData[] Items { get; set; }
        public string? QueryString { get; set; }
        public int PageSize { get; set; }
        public int? ItemsAmount { get; set; }
        public ServiceType ServiceType { get; set; }
        public bool IsSuccesfull { get; set; }
        public string? ConnectionId { get; set; }
    }
}
