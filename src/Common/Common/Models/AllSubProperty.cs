using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllSub.Common.Models
{
    public class AllSubProperty
    {
        public string Id { get; set; } = string.Empty;

        public string Key { get; set; } = string.Empty;

        public string? Value { get; set; }

        public string UserId { get; set; } = string.Empty;
    }
}
