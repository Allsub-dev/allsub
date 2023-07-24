using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;
using System.Security;
using AllSub.Common.Models;

namespace AllSub.WebMVC.Data
{
    public class UserProperty : AllSubProperty
    {
        public UserProperty()
        {
            Id = Guid.NewGuid().ToString();
        }
    }
}
