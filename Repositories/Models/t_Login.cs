using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace Repositories.Models
{
    public class t_Login
    {
        [StringLength(50)]
        public string c_email { get; set; } 

        [StringLength(50)]
        public string c_password { get; set; }
        
    }
}