using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repositories.Models
{
    public class t_ChangePassword
    {
        public int c_userId { get; set; }
        public string c_oldPassword { get; set; }
        public string c_newPassword { get; set; }

        public string c_confirmPassword { get; set; }
    }
}