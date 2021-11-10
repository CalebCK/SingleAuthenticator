
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AxisHumanResource.Models
{
    public class LoginViewModel
    {
        public string Username { get; set; }
        [DataType(DataType.Password)]
        public string Password { get; set; }
        public string ReturnUrl { get; set; }
    }

    public class LoginResponseViewModel
    {
        public string UserId { get; set; }
        public string Username { get; set; }
        public string Surname { get; set; }
        public string Othernames { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string TokenType { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}
