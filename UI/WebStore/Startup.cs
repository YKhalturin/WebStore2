﻿using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Webstore.Clients.Employees;
using WebStore.Clients.Products;
using Webstore.Clients.Values;
using WebStore.DAL.Context;
using WebStore.Domain.Entities.Identity;
using Webstore.Interfaces.Services;
using Webstore.Interfaces.TestAPI;
using Webstore.Services.Data;
using Webstore.Services.Products.InCookies;
using Webstore.Services.Products.InMemory;
using Webstore.Services.Products.InSQL;
using WebStore.Services.Products.InSQL;

namespace WebStore
{
    public class Startup
    {
        private readonly IConfiguration _Configuration;

        public Startup(IConfiguration Configuration) => _Configuration = Configuration;

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<WebStoreDB>(opt => opt.UseSqlServer(_Configuration.GetConnectionString("Default")));
            services.AddTransient<WebStoreDbInitializer>();

            services.AddIdentity<User, Role>(/*opt => { }*/)
               .AddEntityFrameworkStores<WebStoreDB>()
               .AddDefaultTokenProviders();

            services.Configure<IdentityOptions>(opt =>
            {
#if DEBUG
                opt.Password.RequiredLength = 3;
                opt.Password.RequireDigit = false;
                opt.Password.RequireLowercase = false;
                opt.Password.RequireUppercase = false;
                opt.Password.RequireNonAlphanumeric = false;
                opt.Password.RequiredUniqueChars = 3;
#endif
                opt.User.RequireUniqueEmail = false;
                opt.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";

                opt.Lockout.AllowedForNewUsers = true;
                opt.Lockout.MaxFailedAccessAttempts = 10;
                opt.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
            });

            services.ConfigureApplicationCookie(opt =>
            {
                opt.Cookie.Name = "WebStore.GB";
                opt.Cookie.HttpOnly = true;
                opt.ExpireTimeSpan = TimeSpan.FromDays(10);

                opt.LoginPath = "/Account/Login";
                opt.LogoutPath = "/Account/Logout";
                opt.AccessDeniedPath = "/Account/AccessDenied";

                opt.SlidingExpiration = true;
            });
            
            //services.AddTransient<IEmployeesData, InMemoryEmployeesData>();
            services.AddTransient<IEmployeesData, EmployeesClient>();
            //services.AddTransient<IProductData, SqlProductData>();
            services.AddTransient<IProductData, ProductsClient>();
            services.AddScoped<ICartService, InCookiesCartService>();
            services.AddScoped<IOrderService, SqlOrderService>();
            services.AddScoped<IValuesServices, ValuesClient>();

            services
                .AddControllersWithViews()
                .AddRazorRuntimeCompilation();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, WebStoreDbInitializer db)
        {
            db.Initialize();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            
            app.UseAuthorization();

            app.UseWelcomePage("/welcome");

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/greetings", async ctx => await ctx.Response.WriteAsync(_Configuration["greetings"]));
                endpoints.MapGet("/HelloWorld", async ctx => await ctx.Response.WriteAsync("Hello World!"));

                endpoints.MapControllerRoute(
                    name: "areas",
                    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
                );

                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                // http://localhost:5000 -> controller == "Home" action == "Index"
                // http://localhost:5000/Products -> controller == "Products" action == "Index"
                // http://localhost:5000/Products/Page -> controller == "Products" action == "Page"
                // http://localhost:5000/Products/Page/5 -> controller == "Products" action == "Page" id = "5"
            });
        }
    }
}
