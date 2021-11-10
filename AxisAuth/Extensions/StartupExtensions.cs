using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AxisAuth.Repositories;
using AxisAuth.Repositories.IRepositories;
using AxisAuth.Services;
using AxisAuth.Services.IServices;
using Microsoft.Extensions.DependencyInjection;

namespace AxisAuth.Extensions
{
    public static class StartupExtensions
    {

        /// <summary>
        /// Do all Dependency Injections for Repositories with Respective Interfaces here
        /// </summary>
        /// <param name="services"></param>
        public static void InjectRepositories(this IServiceCollection services)
        {
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IClientRepository, ClientRepository>();
        }

        /// <summary>
        /// Do all Dependency Injections for Services with Respective Interfaces here
        /// </summary>
        /// <param name="services"></param>
        public static void InjectServices(this IServiceCollection services)
        {
            services.AddScoped<IAccountService, AccountService>();
        }

    }
}
