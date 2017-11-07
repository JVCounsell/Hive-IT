using Hive_IT.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Hive_IT.Models.Customers
{
    public class CustomerProfileViewModel
    {
        [Required]
        public string Given { get; set; }
        [Required]
        public string Surname { get; set; }
        [Required]
        public DateTime Creation { get; set; }

        public List<CustomerPhoneNumber> Phones { get; set; }
        public List<CustomerEmail> Emails { get; set; }
        public List<CustomerAddress> Addresses { get; set; }
    }
}
