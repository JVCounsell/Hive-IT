﻿using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Hive_IT.Data
{
    public class EditUserViewModel
    {
        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required]
        [MinLength(6)]
        [Display(Name = "Username")]
        public string UserName { get; set; }
               
        [Required]
        [Display(Name = "Role")]
        public string Role { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email Address")]
        public string EmailAddress { get; set; }

        [Required]
        [Phone]
        [MinLength(7), MaxLength(13)]
        [Display(Name = "Phone number")]
        public string PhoneNumber { get; set; }

        [Required]
        public string PreviousUsername { get; set; }

        public List<SelectListItem> RolesList { get; set; }
    }
}
