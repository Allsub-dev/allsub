using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;
using System.Security;

namespace AllSub.WebMVC.Data
{
    public class UserProperty
    {
        public UserProperty()
        {
            Id = Guid.NewGuid().ToString();
        }

        public virtual string Id { get; set; } = default!;

        public string? Key { get; set; }

        public string? Value { get; set; }
        
        public string? UserId { get; set; }
    }
}
