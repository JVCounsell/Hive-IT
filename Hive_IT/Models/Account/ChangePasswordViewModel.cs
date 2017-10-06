using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Hive_IT.Models.Account
{
    public class ChangePasswordViewModel
    {
        //TODO: determine if this is how we want to do it, will require manual implementation of creating an error message if old password is wrong
        [Required]
        [DataType(DataType.Password)]
        public string OldPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Compare("NewPassword", ErrorMessage ="Values do not match")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; }

    }
}
