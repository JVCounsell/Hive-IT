using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Hive_IT.Data
{
    public class ModelofDevice
    {
        [Key]
        public int Identifier { get; set; }

        public string Model { get; set; }

        public int ManufacturerId { get; set; }
        public Manufacturer Manufacturer { get; set; }
    }
}
