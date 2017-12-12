using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Hive_IT.Data
{
    public class WorkOrderHistory
    {
        [Key]
        public string HistoryID { get; set; }

        public string DeviceID { get; set; }
        public string DeviceIdentity { get; set; }

        [Required]
        public string UserId { get; set; }
        [Required]
        public string Username { get; set; }

        [Required]
        public string ActionTaken { get; set; }

        public string FieldValueBefore { get; set; }
        public string FieldValueAfter { get; set; }

        [Required]
        public string TimeOfAction { get; set; }
        
        public string WorkOrderNumber { get; set; }
        public WorkOrder WorkOrder { get; set; }
    }
}
