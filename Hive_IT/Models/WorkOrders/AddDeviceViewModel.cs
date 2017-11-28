using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Hive_IT.Models.WorkOrders
{
    public class AddDeviceViewModel
    {
        public string DeviceId { get; set; }

        [Required]
        public string Status { get; set; }
                
        [Required]
        public string Manufacturer { get; set; }
        public List<SelectListItem> Manufacturers { get; set; }

        [Required]
        public string Model { get; set; }
        public List<SelectListItem> Models { get; set; }

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
    }
}
