using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Hive_IT.Data
{
    public class CustomerEmail
    {      
        [Key]
        public long EmailId { get; set; }
        
        [EmailAddress]
        public string Email { get; set; }

        public long CustomerId { get; set; } 
        public Customer Customer { get; set; }
    }
}
