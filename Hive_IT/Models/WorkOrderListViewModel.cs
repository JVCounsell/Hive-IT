using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hive_IT.Models
{
    public class WorkOrderListViewModel
    {
        public string OrderNumber { get; set; }
        public long CustomerID { get; set; }
        public string CustomerName { get; set; }
        public string OrderStatus { get; set; }
        public string StatusDate { get; set; }
        public int DeviceCount { get; set; }
    }
}
