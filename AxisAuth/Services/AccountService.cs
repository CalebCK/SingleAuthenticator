using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AxisAuth.Extensions;
using AxisAuth.Models.Data.AuthDbContext;
using AxisAuth.Models.ViewModels;
using AxisAuth.Repositories.IRepositories;
using AxisAuth.Services.IServices;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace AxisAuth.Services
{
    public class AccountService : IAccountService
    {
        private readonly IClientRepository _clientRepository;
        private readonly UserManager<AxisUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly string tokenProvider = "Default";
        private readonly string tokenPurpose = "RefreshToken";
        private int tokenLifeSpan = 30;

        public AccountService(IClientRepository clientRepository,
            UserManager<AxisUser> userManager,
            IConfiguration configuration)
        {
            _clientRepository = clientRepository;
            _userManager = userManager;
            _configuration = configuration;
        }

        public async Task<LoginResponseViewModel> LoginAsync(LoginViewModel viewModel)
        {
            var identityUser = await _userManager.FindByNameAsync(viewModel.Username);

            if (identityUser == null)
                throw new BaseException("Invalid login attempt");

            var passwordCheck = await _userManager.CheckPasswordAsync(identityUser, viewModel.Password);

            if (!passwordCheck)
                throw new BaseException("Invalid login attempt");

            var tokenObject = await GenerateTokenObjectAsync(identityUser);

            return tokenObject;
        }

        public async Task RegisterAsync(RegisterViewModel viewModel)
        {
            AxisUser identityUser = new AxisUser 
            { 
                UserName = viewModel.Username, 
                PhoneNumber = viewModel.PhoneNumber, 
                Email = viewModel.Email,
                Surname = viewModel.Surname,
                OtherNames = viewModel.Othernames
            };

            if (!string.IsNullOrEmpty(viewModel.Email) && !ValidateEmail(viewModel.Email))
                throw new BaseException("Provide a valid email address");

            //if (!string.IsNullOrEmpty(viewModel.Email))
            //    identityUser.Email = viewModel.Email;

            var usernameCheck = await _userManager.FindByNameAsync(viewModel.Username);

            if (usernameCheck != null)
                throw new BaseException("Username is already in use");

            try
            {
                var userResult = await _userManager.CreateAsync(identityUser, viewModel.Password);

                if (!userResult.Succeeded)
                {
                    string errorMessage = "";

                    userResult.Errors.ToList().ForEach(x => errorMessage += $" {x.Description}");

                    throw new BaseException(errorMessage);
                }
            }
            catch (Exception ex)
            {
                var userCreated = await _userManager.FindByNameAsync(viewModel.Username);

                if (userCreated != null)
                    await _userManager.DeleteAsync(userCreated);

                throw ex;
            }
        }

        public async Task UpdateAsync(UpdateViewModel viewModel, string userName)
        {

            if (!string.IsNullOrEmpty(viewModel.Email) && !ValidateEmail(viewModel.Email))
                throw new BaseException("Provide a valid email address");


            var axisUser = await _userManager.FindByNameAsync(userName);

            axisUser.PhoneNumber = viewModel.PhoneNumber;
            axisUser.Email = viewModel.Email;
            axisUser.Surname = viewModel.Surname;
            axisUser.OtherNames = viewModel.Othernames;

            var userResult = await _userManager.UpdateAsync(axisUser);

            if (!userResult.Succeeded)
            {
                string errorMessage = "";

                userResult.Errors.ToList().ForEach(x => errorMessage += $" {x.Description}");

                throw new BaseException(errorMessage);
            }
            
        }

        public void ValidateClient(ClientViewModel viewModel)
        {
            if (!IsClientValid(viewModel.ClientId, viewModel.ClientSecret))
                throw new Exception($"Invalid Client with ID {viewModel.ClientId} and secret {viewModel.ClientSecret}");
        }

        #region Private Methods

        private bool ValidateEmail(string input)
        {
            try
            {
                MailAddress mail = new MailAddress(input.Trim());
                return true;
            }
            catch
            {
                return false;
            }
        }

        private async Task<LoginResponseViewModel> GenerateTokenObjectAsync(AxisUser user)
        {
            tokenLifeSpan = Convert.ToInt32(_configuration.GetSection("Misc")["TokenLifeSpanMinutes"]);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(JwtRegisteredClaimNames.Nbf, new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds().ToString()),
                new Claim(JwtRegisteredClaimNames.Exp, new DateTimeOffset(DateTime.Now.AddMinutes(tokenLifeSpan)).ToUnixTimeSeconds().ToString()),
            };

            var tokenSecret = _configuration.GetSection("Misc")["TokenSecret"];
            var jwtPayLoad = new JwtPayload(claims);
            var symSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenSecret));
            var signingCredentials = new SigningCredentials(symSecurityKey, SecurityAlgorithms.HmacSha256);
            var jwtHeader = new JwtHeader(signingCredentials);

            var token = new JwtSecurityToken(jwtHeader, jwtPayLoad);
            var refreshToken = await GenerateRefreshTokenAsync(user);
            await SaveRefreshTokenAsync(user, refreshToken);

            LoginResponseViewModel output = new LoginResponseViewModel
            {
                TokenType = "Bearer",
                AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
                RefreshToken = refreshToken,
                Username = user.UserName,
                UserId = user.Id,
                Email = user.Email,
                Surname = user.Surname,
                Othernames = user.OtherNames,
                PhoneNumber = user.PhoneNumber
            };

            return output;
        }

        private async Task<string> GenerateRefreshTokenAsync(AxisUser user)
        {
            var token = await _userManager.GenerateUserTokenAsync(user, tokenProvider, tokenPurpose);
            return token;
        }

        private bool IsClientValid(string clientId, string clientSecret)
        {
            return _clientRepository.Find(x => x.ClientId.ToLower().Trim() == clientId.ToLower().Trim() && x.ClientSecret.ToString().ToLower().Trim() == clientSecret.ToLower().Trim()).Any();
        }

        private async Task SaveRefreshTokenAsync(AxisUser appUser, string refreshToken)
        {
            var result = await _userManager.SetAuthenticationTokenAsync(appUser, tokenProvider, tokenPurpose, refreshToken);
            if (!result.Succeeded)
            {
                throw new Exception("Failed to create refresh token");
            }
        }

        private async Task RemoveRefreshTokenAsync(AxisUser user)
        {
            var result = await _userManager.RemoveAuthenticationTokenAsync(user, tokenProvider, tokenPurpose);
            if (!result.Succeeded)
            {
                throw new Exception("Failed to remove existing refresh token");
            }
        }

        private async Task<bool> VerifyRefreshTokenAsync(AxisUser user, string refreshToken)
        {
            return await _userManager.VerifyUserTokenAsync(user, tokenProvider, tokenPurpose, refreshToken);
        }

        #endregion
    }
}
