using GlamStudio.Data;
using GlamStudio.Models;
using GlamStudio.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

namespace GlamStudio
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<BeautySalonContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
            );

            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireDigit = false;
            })
             .AddEntityFrameworkStores<BeautySalonContext>()
             .AddDefaultTokenProviders();

            builder.Services.AddTransient<IEmailSender, EmailSender>();

            builder.Services.AddRazorPages();

            var app = builder.Build();

            // Tworzenie ról po zbudowaniu aplikacji
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                await CreateRolesAsync(services);
                await CreateAdminUserAsync(services);
            }

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapRazorPages();
            app.MapControllerRoute(
                name: "areas",
                pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
            );
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}"
            );

            app.Run();
        }

        private static async Task CreateRolesAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            string[] roleNames = { "Admin", "Employee", "Client" };

            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }

        private static async Task CreateAdminUserAsync(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            string adminEmail = "admin@glamstudio.pl";
            string adminPassword = "ZaTrudneHaslo!123";
            string adminFirstName = "Admin";
            string adminLastName = "GlamStudio";

            if (!await roleManager.RoleExistsAsync("Admin") || !await roleManager.RoleExistsAsync("Employee"))
            {
                return;
            }

            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                var newAdmin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = adminFirstName,
                    LastName = adminLastName,
                    EmailConfirmed = true,
                };

                var result = await userManager.CreateAsync(newAdmin, adminPassword);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(newAdmin, "Admin");
                    await userManager.AddToRoleAsync(newAdmin, "Employee");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        Console.WriteLine($"B£¥D SEEDOWANIA ADMINA: {error.Description}");
                    }
                }
            }
        }
    }
}
