using Hive_IT.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hive_IT.Models.WorkOrders
{
    public class InvoiceViewModel
    {
        public string OrderNumber { get; set; }
        public string Status { get; set; }
        public List<SelectListItem> StatusList { get; set; }

        public string CustomerName { get; set; }
        public CustomerEmail Email { get; set; }
        public CustomerPhoneNumber Phone { get; set; }
        public CustomerAddress ShippingAddress { get; set; }

        public List<Device> LinkedDevices { get; set; }

        public List<Service> ServicesDone { get; set; }
        public List<SelectListItem> AvailableServices { get; set; }
        public Dictionary<int, int> NumberOfServices { get; set; }
        public float AmountDue { get; set; }
    }
}
