using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AxisAuth.Extensions;
using AxisAuth.Models.Data.AuthDbContext;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace AxisAuth
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ClientDbContext>(options =>
                    options.UseSqlServer(Configuration.GetConnectionString("AuthDbConnection")));

            services.AddDbContext<AxisAuthDbContext>(options =>
                    options.UseSqlServer(Configuration.GetConnectionString("AuthDbConnection")));

            services.AddDefaultIdentity<AxisUser>(o =>
            {
                o.SignIn.RequireConfirmedEmail = false;
                // configure password options (e.g: 4abcde)
                o.Password.RequireDigit = true;
                //o.Password.RequireLowercase = false;
                o.Password.RequireUppercase = false;
                o.Password.RequireNonAlphanumeric = false;
                o.Password.RequiredLength = 6;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AxisAuthDbContext>()
            .AddDefaultTokenProviders();

            //add authentication
            services.AddAuthentication(o =>
            {
                o.DefaultAuthenticateScheme = "JwtBearer";
                o.DefaultChallengeScheme = "JwtBearer";
            }).AddJwtBearer("JwtBearer", j =>
            {
                j.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration.GetSection("Misc")["TokenSecret"])),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(5)
                };
            });

            services.InjectServices();
            services.InjectRepositories();

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //custom exception handler
            app.UseExceptionHandler(a => a.Run(async context =>
            {
                var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
                var exception = exceptionHandlerPathFeature.Error;


                var result = JsonConvert.SerializeObject(new { error = exception.Message });
                
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

                if (exception is BaseException || exception is SecurityTokenException || exception.InnerException is BaseException)
                {
                    await context.Response.WriteAsync(result);
                    return;
                }

                //log exception if it's not a BaseException; thus it wasn't anticipated or the error details should only be available to system admin
                string errMessage = CoreExtensions.InterceptException(exception, exceptionHandlerPathFeature.Path, /*exception.Error.StackTrace*/new StackTrace(), user: string.Empty);
                var customeResult = JsonConvert.SerializeObject(new { error = errMessage });
                await context.Response.WriteAsync(customeResult);
            }));

            app.UseAuthentication();
            app.UseHttpsRedirection();

            app.UseStaticFiles();
            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
