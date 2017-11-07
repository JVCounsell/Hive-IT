using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Hive_IT.Data
{
    public class Customer
    {
        public long CustomerId { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        public DateTime DateCreated { get; set; }

        public virtual ICollection<CustomerPhoneNumber> CustomerPhoneNumber { get; set; }
        public virtual ICollection<CustomerEmail> CustomerEmail { get; set; }
        public virtual ICollection<CustomerAddress> CustomerAddress { get; set; }
    }
}
