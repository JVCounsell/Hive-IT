using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Hive_IT.Data
{
    public class WorkOrder
    {
        [Key]
        [Display(Name = "Work Order Number")]
        public string WorkOrderNumber { get; set; }

        [Required]
        public string Status { get; set; }
        
        public string TimeCreated {get; set;}
        public string PaidAt { get; set; }
        public string CompletionAt { get; set; }

        public virtual ICollection<Device> Device { get; set; }

        public long CustomerId { get; set; }
        public Customer Customer { get; set; }
    }
}
