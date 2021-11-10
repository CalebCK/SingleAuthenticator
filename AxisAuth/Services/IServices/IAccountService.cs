using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AxisAuth.Models.ViewModels;

namespace AxisAuth.Services.IServices
{
    /// <summary>
    /// Business logic of accounts management
    /// </summary>
    public interface IAccountService
    {
        Task<LoginResponseViewModel> LoginAsync(LoginViewModel viewModel);
        Task RegisterAsync(RegisterViewModel viewModel);
        Task UpdateAsync(UpdateViewModel viewModel, string userName);
        void ValidateClient(ClientViewModel viewModel);
    }
}
