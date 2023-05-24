using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AllSub.CommonCore.Models
{
    public record IntegrationEvent
    {
        public IntegrationEvent()
        {
            Id = Guid.NewGuid();
            CreationDate = DateTime.UtcNow;
            DataDictionary = new Dictionary<string, string>();
        }

        [JsonConstructor]
        public IntegrationEvent(Guid id, DateTime createDate, Dictionary<string, string> dataDictionary)
        {
            Id = id;
            CreationDate = createDate;
            DataDictionary = dataDictionary;
        }

        [JsonInclude]
        public Guid Id { get; private init; }

        [JsonInclude]
        public DateTime CreationDate { get; private init; }

        [JsonInclude]
        public IDictionary<string, string> DataDictionary { get; set; }
    }
}
