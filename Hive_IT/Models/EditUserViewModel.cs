using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Hive_IT.Models
{
    public class EditUserViewModel
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

        //TODO: make this only available to admin and probably manager
        [Required]
        [Display(Name = "Role")]
        public string Role { get; set; }

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
