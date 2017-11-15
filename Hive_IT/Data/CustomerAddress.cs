using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Hive_IT.Data
{
    public class CustomerAddress
    {
        [Key]
        public long AddressId { get; set; }
        
        [Display(Name = "Street Address")]
        public string StreetAddress { get; set; }

        public string City { get; set; }
        
        [Display(Name = "Province/State")]
        public string ProvState { get; set; }
               
        public string Country { get; set; }
                
        [MinLength(5), MaxLength(6)]
        [RegularExpression(@"[0-9]{5}|[a-zA-Z][0-9][a-zA-Z][0-9][a-zA-Z][0-9]", 
            ErrorMessage = "Format is 5 digits or letter, digit, letter, digit, letter, digit.")]
        [Display(Name = "Postal/Zip Code")]
        public string Postal { get; set; }

        public long CustomerId { get; set; }
        public Customer Customer { get; set; }
    }
}
