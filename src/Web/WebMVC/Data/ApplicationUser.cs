using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace AllSub.WebMVC.Data
{
    public class ApplicationUser : IdentityUser
    {
        public ApplicationUser() 
        {
            UserProperties = new List<UserProperty>();
        }

        public virtual ICollection<UserProperty> UserProperties { get; set; }
    }
}
