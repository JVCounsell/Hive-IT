using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Hive_IT.Data
{
    public class Service
    {       
        public int ServiceId { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public float Price { get; set; }

        public string Manufacturer { get; set; }
        public string Model { get; set; }

        public virtual ICollection<WorkOrderService> WorkOrderServices { get; set; }
    }
}
