using Hive_IT.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hive_IT.Models.WorkOrders
{
    public class OrderViewModel
    {
        public string OrderNumber { get; set; }
        public string Status { get; set; }
        public List<SelectListItem> StatusList {get; set;}
        public string StatusChangedAt { get; set; }

        public string CustomerName { get; set; }
        public long CustomerID { get; set; }

        public List<CustomerEmail> Emails { get; set; }
        public List<CustomerPhoneNumber> Phones { get; set; }
        public List<Device> LinkedDevices { get; set; }
    }
}
