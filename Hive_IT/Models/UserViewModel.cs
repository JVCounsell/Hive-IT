using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Hive_IT.Models
{
    public class UserViewModel
    {
   
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }        
        public string Role { get; set; }
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
        public string CreationDate { get; set; }
    }
}
