using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using IPGManager.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using IPGManager.Models;

namespace IPGManager
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
            services.AddDbContext<ApplicationDbContext>(options =>
                 options.UseSqlServer(
                     Configuration.GetConnectionString("DefaultConnection")));

            //services.AddDefaultIdentity<IdentityUser>(
            //    options => options.SignIn.RequireConfirmedAccount = false
            //).AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders()
                .AddDefaultUI();

            services.Configure<IdentityOptions>(
                options => {
                    // Password settings
                    options.Password.RequireDigit = true;
                    options.Password.RequiredLength = 8;
                    options.Password.RequireNonAlphanumeric = true;
                    options.Password.RequireUppercase = true;
                    options.Password.RequireLowercase = true;

                    // Lockout
                    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                    options.Lockout.MaxFailedAccessAttempts = 5;
                    options.Lockout.AllowedForNewUsers = true;

                    // Users
                    options.User.RequireUniqueEmail = true;

                    // Sign in
                    options.SignIn.RequireConfirmedAccount = false;
                }
            );

            services.AddAuthorization(
                options => {
                    options.AddPolicy(
                        "CanManageFuncionarios",
                        policy => policy.RequireRole("manager", "admin")
                    );

                    // other policies ...
                }
            );

            services.AddControllersWithViews();
            services.AddRazorPages();        

        services.AddDbContext<IPGManagerDBContext>(options =>
                    options.UseSqlServer(Configuration.GetConnectionString("IPGManagerDBContext")));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                using 
                    (var serviceScope = app.ApplicationServices.CreateScope()) { 
                    var db = serviceScope.ServiceProvider.GetService<IPGManagerDBContext>(); 
                    SeedData.Populate(db); 
                }
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();

            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });
        }
    }
}
