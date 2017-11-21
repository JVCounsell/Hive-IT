using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Hive_IT.Data
{
    public class Device
    {
        [Key]
        public string DeviceId { get; set; }

        [Required]
        public string Status { get; set; }
        
        public string CreatedAt { get; set; }
        public string DiagnosedAt { get; set; }
        public string BegunRepairAt { get; set; }
        public string RepairedAt { get; set; }
        public string DeclaredUnfixableAt { get; set; }
        public string PickedUpAt { get; set; }

        [Required]
        public string Manufacturer { get; set; }

        [Required]
        public string Model { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string Provider { get; set; }

        [Required]
        [Display(Name = "Serial Number")]
        public string Serial { get; set; }

        [Required]
        public string Problem { get; set; }
        public string Notes { get; set; }

        public string WorkOrderNumber { get; set; }
        public WorkOrder WorkOrder { get; set; }
    }
}
