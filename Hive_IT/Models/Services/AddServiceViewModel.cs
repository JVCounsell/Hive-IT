using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Hive_IT.Models.Services
{
    public class AddServiceViewModel
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public float Price { get; set; }

        public string Manufacturer { get; set; }
        public string Model { get; set; }

        [Required]
        public bool NotSpecific { get; set; }

        public List<SelectListItem> ManufacturerList { get; set; }
        public List<SelectListItem> ModelList { get; set; }
    }
}
