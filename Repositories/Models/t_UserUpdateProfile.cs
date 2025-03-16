using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Repositories.Models
{
    public class t_UserUpdateProfile
    {
        public int c_userId { get; set; }
        public string c_email { get; set; }
        public string? c_image { get; set; }

        public IFormFile? ImageFile { get; set; }  // Prevents from being mapped to the database

        public string newImage { get; set; }
        
    }
}