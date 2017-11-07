using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Hive_IT.Data
{
    public class CustomerAddress
    {
        public long Id { get; set; }
        
        [Display(Name = "Street Address")]
        public string StreetAddress { get; set; }

        public string City { get; set; }
        
        [Display(Name = "Province/State")]
        public string ProvState { get; set; }
               
        public string Country { get; set; }
                
        [MinLength(5), MaxLength(6)]
        [Display(Name = "Postal/Zip Code")]
        public string Postal { get; set; }

        public long CustomerId { get; set; }
        public Customer Customer { get; set; }
    }
}
