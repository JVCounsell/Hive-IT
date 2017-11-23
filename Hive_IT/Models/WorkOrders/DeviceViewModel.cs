using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Hive_IT.Models.WorkOrders
{
    public class DeviceViewModel
    {       
        public string DeviceNumber { get; set; }

        [Required]
        public string Status { get; set; }
        public string StatusLastUpdatedAt { get; set; }

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
        
        public List<SelectListItem> StatusList { get; set; }
        public string OrderNumber { get; set; }
    }
}
