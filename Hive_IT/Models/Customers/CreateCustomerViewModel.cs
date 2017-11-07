using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Hive_IT.Models.Customers
{
    public class CreateCustomerViewModel
    {
        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [EmailAddress]
        [Display(Name = "Email Address")]
        public string Email { get; set; }

        [Phone]
        [Display(Name = "Phone Number")]
        [MaxLength(13), MinLength(7)]
        public string PhoneNumber { get; set; }

        [Display(Name = "Street Address")]
        public string StreetAddress { get; set; }

        public string City { get; set; }
        
        [Display(Name = "Province/State")]
        public string ProvState { get; set; }
        
        public string Country { get; set; }
        
        [MinLength(5), MaxLength(6)]
        [Display(Name = "Postal/Zip Code")]
        public string Postal { get; set; }
    }
}
