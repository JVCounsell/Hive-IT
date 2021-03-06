﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Hive_IT.Models.Account
{
    public class ResetPasswordViewModel
    {
        [Required]
        [Display(Name = "New Temporary Password")]
        [DataType(DataType.Password)]
        public string NewTemporaryPassword { get; set; }

        [Compare("NewTemporaryPassword", ErrorMessage = "Passwords must match!")]
        [Display(Name ="Confirm Temporary Password")]
        public string ConfirmTemporaryPassword { get; set; }

        [Required]
        public string Username { get; set; }
    }
}
