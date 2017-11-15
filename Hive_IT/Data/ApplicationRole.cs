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
        [RegularExpression(@"^[a-zA-Z][a-zA-z0-9-_.,]+$",
            ErrorMessage = "First character must be a letter and the rest must be alphanumeric or one of (_-.,)") ]
        [Display(Name = "Name")]
        public string Name { get; set; }
    }
}
