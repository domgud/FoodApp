using FoodApp.Data;
using FoodApp.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vereyon.Web;
using FoodApp.Helpers;
using static FoodApp.Models.Restaurant;

namespace FoodApp
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
            services.AddDefaultIdentity<IdentityUser>(options => {
                options.SignIn.RequireConfirmedAccount = false;
                options.SignIn.RequireConfirmedEmail = false;
                options.Password.RequireDigit = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
            }).AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();
            services.AddIdentityCore<Restaurant>(options => {
                options.SignIn.RequireConfirmedAccount = false;
                options.SignIn.RequireConfirmedEmail = false;
                options.Password.RequireDigit = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
            }).AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();
            services.AddControllersWithViews();
            services.AddRazorPages();
            services.AddHttpContextAccessor();
            services.AddFlashMessage();
            services.AddSession();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider)
        {
            if (env.IsDevelopment())
            {
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
            app.UseSession();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });
            

            //run migrations automatically?
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            context.Database.Migrate();
            CreateRoles(serviceProvider);
            APIHelper.InitializeClient();
        }
        private void CreateRoles(IServiceProvider serviceProvider)
        {

            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            Task<IdentityResult> roleResult;
            //add the roles if they don't exist
            string[] roleNames = { "Administrator", "Restaurant", "User" };
            foreach (var roleName in roleNames)
            {
                Task<bool> roleExists = roleManager.RoleExistsAsync(roleName);
                roleExists.Wait();

                if (!roleExists.Result)
                {
                    roleResult = roleManager.CreateAsync(new IdentityRole(roleName));
                    roleResult.Wait();
                }
            }

            
            CreateUser(userManager, "admin@admin", "Administrator");
            CreateClient(userManager, "user@user", "User");
            CreateRestaurant(userManager, "restaurant@restaurant", "Restaurant", RestaurantState.Confirmed);
            CreateRestaurant(userManager, "restaurant2@restaurant2", "Restaurant", RestaurantState.Pending);
            CreateRestaurant(userManager, "restaurant3@restaurant3", "Restaurant", RestaurantState.Pending);

        }
        private void CreateUser(UserManager<IdentityUser> userManager, string email, string role)
        {
            Task<IdentityUser> testUser = userManager.FindByEmailAsync(email);
            testUser.Wait();

            if (testUser.Result == null)
            {
                
                IdentityUser user = new IdentityUser();
                user.Email = email;
                user.UserName = email;
               
                Task<IdentityResult> newUser = userManager.CreateAsync(user, "password");
                newUser.Wait();

                if (newUser.Result.Succeeded)
                {
                    Task<IdentityResult> newUserRole = userManager.AddToRoleAsync(user, role);
                    newUserRole.Wait();
                }
            }
        }
        private void CreateClient(UserManager<IdentityUser> userManager, string email, string role)
        {
            Task<IdentityUser> testUser = userManager.FindByEmailAsync(email);
            testUser.Wait();

            if (testUser.Result == null)
            {

                Client user = new Client();
                user.Email = email;
                user.UserName = email;
                user.Address = "Kaunas, Studentu g. 71";
                user.Name = "Petras";

                Task<IdentityResult> newUser = userManager.CreateAsync(user, "password");
                newUser.Wait();

                if (newUser.Result.Succeeded)
                {
                    Task<IdentityResult> newUserRole = userManager.AddToRoleAsync(user, role);
                    newUserRole.Wait();
                }
            }
        }
        private void CreateRestaurant(UserManager<IdentityUser> userManager, string email, string role, RestaurantState state)
        {
            Task<IdentityUser> testUser = userManager.FindByEmailAsync(email);
            testUser.Wait();

            if (testUser.Result == null)
            {

                Restaurant user = new Restaurant();
                user.Email = email;
                user.UserName = email;
                user.Address = "Kaunas";
                user.Name = "Liuks";
                user.State = state;

                Task<IdentityResult> newUser = userManager.CreateAsync(user, "password");
                newUser.Wait();

                if (newUser.Result.Succeeded)
                {
                    Task<IdentityResult> newUserRole = userManager.AddToRoleAsync(user, role);
                    newUserRole.Wait();
                }
            }
        }

    }
}
