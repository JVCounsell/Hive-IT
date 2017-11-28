using Hive_IT.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hive_IT.Models.MakeandModel
{
    public class ManufacturerModelViewModel
    {
        public int IDNumber { get; set; }
        public string ManufacturerName { get; set; }

        public List<ModelofDevice> LinkedModels { get; set; }
    }
}
