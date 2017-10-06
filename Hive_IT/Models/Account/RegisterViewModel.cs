using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Hive_IT.Models.Account
{
    public class RegisterViewModel
    {
        [Required]
        [MaxLength(25)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(30)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required]
        [MaxLength(30), MinLength(8)]
        [Display(Name = "Username")]
        public string UserName { get; set; }

        [Required]
        [Display(Name = "Role")]
        public string Role { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "Passwords must match!")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; }

        [Required]
        [EmailAddress, MaxLength(50)]
        [Display(Name = "Email Address")]
        public string EmailAddress { get; set; }

        [Required]
        [Phone]
        [Display(Name = "Phone number")]
        public string PhoneNumber { get; set; }    
        
        public List<SelectListItem> RolesList { get; set; }
    }
}
