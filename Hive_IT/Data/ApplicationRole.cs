using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Hive_IT.Data
{
    public class ApplicationRole
    {
        [Required]
        [Display(Name = "Name")]
        public string Name { get; set; }
    }
}
