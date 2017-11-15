using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hive_IT.Models.Customers
{
    public class ListedCustomerViewModel
    {
        public string GivenName { get; set; }
        public string Surname { get; set; }
        public string Created { get; set; }
        public long ItemID { get; set; }
        public string FirstEmail { get; set; }
        public string FirstPhone { get; set; }
    }
}
