﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Hive_IT.Data
{
    public class CustomerPhoneNumber
    {
        [Key]
        public long PhoneId { get; set; }

        [Required]
        [Phone]
        [MaxLength(13), MinLength(7)]
        public string PhoneNumber { get; set; }

        public long CustomerId { get; set; }
        public Customer Customer { get; set; }
        
    }
}
