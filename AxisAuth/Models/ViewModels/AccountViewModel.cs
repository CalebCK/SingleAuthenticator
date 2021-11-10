using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace AxisAuth.Models.ViewModels
{
    /// <summary>
    /// View model for login credentials
    /// </summary>
    public class LoginViewModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    /// <summary>
    /// Client model that extracts corresponding values from header attributes
    /// </summary>
    public class ClientViewModel
    {
        [FromHeader]
        public string ClientId { get; set; }
        [FromHeader]
        public string ClientSecret { get; set; }
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

    public class RegisterViewModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Surname { get; set; }
        public string Othernames { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
    }

    public class UpdateViewModel
    {
        public string Surname { get; set; }
        public string Othernames { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
    }
}
