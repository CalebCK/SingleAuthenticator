using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace AxisAuth.Models.Data.AuthDbContext
{
    /// <summary>
    /// Custom user that extends identity user
    /// </summary>
    public class AxisUser : IdentityUser
    {
        [MaxLength(50)]
        public string Surname { get; set; }
        [MaxLength(150)]
        public string OtherNames { get; set; }
    }
}
