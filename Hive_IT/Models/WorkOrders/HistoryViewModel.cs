using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hive_IT.Models.WorkOrders
{
    public class HistoryViewModel
    {
        public string HistId { get; set; }
        public string DeviceID { get; set; }
        public string DeviceSerialIdentifier { get; set; }
        public bool IsDeviceActive { get; set; }
       
        public string Username { get; set; }
        
        public string ActionTaken { get; set; }

        public string FieldValueBefore { get; set; }
        public string FieldValueAfter { get; set; }
       
        public string TimeOfAction { get; set; }

        public string WorkOrderNumber { get; set; }
    }
}
