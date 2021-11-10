using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AxisAuth.Models.ViewModels;
using AxisAuth.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AxisAuth.Controllers
{
    [Route("api/[controller]")]
    [Produces("application/json")]
    [ApiController]
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        /// <summary>
        /// Login user, returns data and token if successful
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        [Route("login")]
        [HttpPost]
        public ActionResult<LoginResponseViewModel> Login(LoginViewModel viewModel, [FromHeader] ClientViewModel clientModel)
        {
            _accountService.ValidateClient(clientModel);

            var response = _accountService.LoginAsync(viewModel).Result;

            return response;
        }

        /// <summary>
        /// Welcome page
        /// </summary>
        /// <returns></returns>
        [Route("index")]
        [HttpGet]
        public IActionResult Index()
        {
            return Content("Welcome to Axis Pension's Authentication API!");
        }

        /// <summary>
        /// Register new user; accessible to all other clients
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        [Route("register")]
        [HttpPost]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterViewModel viewModel, [FromHeader] ClientViewModel clientModel)
        {
            _accountService.ValidateClient(clientModel);

            await _accountService.RegisterAsync(viewModel);

            return Ok();
        }

        /// <summary>
        /// Update existing user; accessible to all authorized other clients
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        [Route("update")]
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UpdateAsync(UpdateViewModel viewModel, [FromHeader] ClientViewModel clientModel)
        {
            _accountService.ValidateClient(clientModel);

            await _accountService.UpdateAsync(viewModel, User.Identity.Name);

            return Ok();
        }
    }
}
