using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Hive_IT.Models.Account
{
    public class LoginViewModel
    {
        [Required]
        [MaxLength(30), MinLength(8)]
        [Display(Name = "Username")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "Passwords must match!")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; }

    }
}
