using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AxisHumanResource.Extensions;
using AxisHumanResource.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using RestSharp;

namespace AxisHumanResource.Controllers
{
    public class AccountController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly SignInManager<IdentityUser> _signInManager;
        private string _authBaseUrl;
        private string _clientId;
        private string _clientSecret;
        private string _tokenSecret;

        public AccountController(IConfiguration configuration, SignInManager<IdentityUser> signInManager)
        {
            _configuration = configuration;
            _signInManager = signInManager;
            _clientId = _configuration.GetSection("Authentication")["ClientId"];
            _clientSecret = _configuration.GetSection("Authentication")["ClientSecret"];
            _tokenSecret = _configuration.GetSection("Authentication")["TokenSecret"];
            _authBaseUrl = _configuration.GetSection("Authentication")["AuthenticatorBaseUrl"];

        }

        [HttpGet]
        public IActionResult Login(string returnUrl)
        {
            returnUrl = returnUrl ?? "/";
            LoginViewModel model = new LoginViewModel() { ReturnUrl = returnUrl};
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var loginUrl = $"{_authBaseUrl}/api/account/login";
                RestClient authClient = new RestClient(loginUrl);

                //create body for request which will be passes as a json object
                dynamic requestBody = new
                {
                    Username = viewModel.Username,
                    Password = viewModel.Password
                };

                var request = new RestRequest(Method.POST);
                request.AddHeader("Cache-Control", "no-cache");
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("ClientId", _clientId);
                request.AddHeader("ClientSecret", _clientSecret);
                request.AddJsonBody(requestBody);

                IRestResponse restResponse = authClient.Execute(request);

                if (restResponse.IsSuccessful)
                {
                    //convert response content to LoginResponseViewModel
                    LoginResponseViewModel loginResponse = JsonConvert.DeserializeObject<LoginResponseViewModel>(restResponse.Content);

                    //set response in session 
                    HttpContext.Session.Set(SessionValueKeys.loginResponseData, loginResponse);
                    HttpContext.Session.Set(SessionValueKeys.userId, loginResponse.UserId);
                    HttpContext.Session.Set(SessionValueKeys.userName, loginResponse.Username);

                    //get user claims from access token
                    var returnPrincipal = ValidateToken(loginResponse.AccessToken);
                    var claims = returnPrincipal.Claims;

                    var userIdentity = new ClaimsIdentity(claims, "login");
                    
                    ClaimsPrincipal principal = new ClaimsPrincipal(userIdentity);

                    //sign in user
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
                    return LocalRedirect(viewModel.ReturnUrl);
                }

                //convert response content to error
                ErrorResponseViewModel errorResponse = JsonConvert.DeserializeObject<ErrorResponseViewModel>(restResponse.Content);

                ModelState.AddModelError(string.Empty, errorResponse.Error);
                return View(viewModel);
            }
            else
            {
                ModelState.AddModelError(string.Empty, GetModelStateErrors(ModelState));
                return View(viewModel);
            }
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(RegisterViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var loginUrl = $"{_authBaseUrl}/api/account/register";
                RestClient authClient = new RestClient(loginUrl);

                //create request body to be sent as a json object
                dynamic requestBody = new
                {
                    Username = viewModel.Username,
                    Password = viewModel.Password,
                    Surname = viewModel.Surname,
                    Othernames = viewModel.Othernames,
                    Email = viewModel.Email,
                    PhoneNumber = viewModel.PhoneNumber
                };

                var request = new RestRequest(Method.POST);
                request.AddHeader("Cache-Control", "no-cache");
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("ClientId", _clientId);
                request.AddHeader("ClientSecret", _clientSecret);
                request.AddJsonBody(requestBody);

                IRestResponse restResponse = authClient.Execute(request);

                if (restResponse.IsSuccessful)
                {
                    //redirect user to login with new credentials
                    return RedirectToAction("Login");
                }

                //convert response content to error
                ErrorResponseViewModel errorResponse = JsonConvert.DeserializeObject<ErrorResponseViewModel>(restResponse.Content);

                ModelState.AddModelError(string.Empty, errorResponse.Error);
                return View(viewModel);
            }
            else
            {
                ModelState.AddModelError(string.Empty, GetModelStateErrors(ModelState));
                return View(viewModel);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await _signInManager.SignOutAsync();
            HttpContext.Response.Cookies.Delete(".AspNetCore.Cookies");
            await HttpContext.SignOutAsync("Cookies");

            return RedirectToAction("Login");
        }

        /// <summary>
        /// Generate errors with model state
        /// </summary>
        /// <param name="modelState"></param>
        /// <returns></returns>
        private string GetModelStateErrors(ModelStateDictionary modelState)
        {
            string error = "";

            foreach (var item in modelState.Values)
            {
                foreach (var err in item.Errors)
                {
                    error = error + $"{err.ErrorMessage};" + Environment.NewLine;
                }
            }

            return error;
        }

        /// <summary>
        /// Valid access token and also generate claims from token
        /// </summary>
        /// <param name="jwtToken"></param>
        /// <returns></returns>
        private ClaimsPrincipal ValidateToken(string jwtToken)
        {
            IdentityModelEventSource.ShowPII = true;

            SecurityToken validatedToken;
            TokenValidationParameters validationParameters = new TokenValidationParameters();

            validationParameters.ValidateLifetime = true;
            validationParameters.ValidateAudience = false;
            validationParameters.ValidateIssuer = false;

            //validationParameters.ValidAudience = "";
            //validationParameters.ValidIssuer = _"";
            validationParameters.IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_tokenSecret));

            ClaimsPrincipal principal = new JwtSecurityTokenHandler().ValidateToken(jwtToken, validationParameters, out validatedToken);

            return principal;
        }
    }
}
