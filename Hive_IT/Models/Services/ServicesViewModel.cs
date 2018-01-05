using Hive_IT.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hive_IT.Models.Services
{
    public class ServicesViewModel
    {
       public List<Service> Services { get; set; }
       public List<SelectListItem> ManufacturerList { get; set; }
       public List<SelectListItem> ModelList { get; set; }
       public bool Specific { get; set; }

       public string Manufacturer { get; set; }
       public string Model { get; set; }
    }
}
