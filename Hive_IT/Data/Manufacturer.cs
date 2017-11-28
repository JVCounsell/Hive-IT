using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Hive_IT.Data
{
    public class Manufacturer
    {
        public int ManufacturerId { get; set; }

        [Display(Name = "Manufacturer Name")]
        public string ManufacturerName { get; set; }

        public virtual ICollection<ModelofDevice> Models { get; set; }
    }
}
