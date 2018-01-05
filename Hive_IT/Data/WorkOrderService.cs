using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hive_IT.Data
{
    public class WorkOrderService
    {
        public int ServiceId { get; set; }
        public string WorkOrderNumber { get; set; }

        public WorkOrder WorkOrder { get; set; }
        public Service Service { get; set; }
    }
}
