using DevDiary.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace DevDiary.Data
{
    public static class DataUtility
    {

        private const string? _adminRole = "Admin";
        private const string? _moderatorRole = "Moderator";

        public static string GetConnectionString(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
            return string.IsNullOrEmpty(databaseUrl) ? connectionString! : BuildConnectionString(databaseUrl);
        }


        private static string BuildConnectionString(string databaseUrl)
        {
            //Provides an object representation of a uniform resource identifier (URI) and easy access to the parts of the URI.
            var databaseUri = new Uri(databaseUrl);
            var userInfo = databaseUri.UserInfo.Split(':');
            //Provides a simple way to create and manage the contents of connection strings used by the NpgsqlConnection class.
            var builder = new NpgsqlConnectionStringBuilder
            {
                Host = databaseUri.Host,
                Port = databaseUri.Port,
                Username = userInfo[0],
                Password = userInfo[1],
                Database = databaseUri.LocalPath.TrimStart('/'),
                SslMode = SslMode.Prefer,
                TrustServerCertificate = true
            };
            return builder.ToString();
        }

        public static async Task ManageDataAsync(IServiceProvider svcProvider)
        {
            // Obtaining the necessary services based on the IServiceProvider parameter
            var dbContextSvc = svcProvider.GetRequiredService<ApplicationDbContext>();
            var userManagerSvc = svcProvider.GetRequiredService<UserManager<AppUser>>();
            var configurationSvc = svcProvider.GetRequiredService<IConfiguration>();
            var roleManagerSvc = svcProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Align the database by checking Migrations
            await dbContextSvc.Database.MigrateAsync();

            // Seed Application Roles
            await SeedRolesAsync(roleManagerSvc);

            //Seed User(s)
            await SeedAppUsersAsync(userManagerSvc, configurationSvc);

            // Seed Demo User(s)
            await SeedDemoUsersAsync(userManagerSvc, configurationSvc);
        }



        // Admin & moderator
        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            if (!await roleManager.RoleExistsAsync(_adminRole!))
            {
                await roleManager.CreateAsync(new IdentityRole(_adminRole!));
            }
            if (!await roleManager.RoleExistsAsync(_moderatorRole!))
            {
                await roleManager.CreateAsync(new IdentityRole(_moderatorRole!));
            }
        }


        private static async Task SeedAppUsersAsync(UserManager<AppUser> userManager, IConfiguration configuration)
        {

            string? adminEmail = configuration["AdminLoginEmail"] ?? Environment.GetEnvironmentVariable("AdminLoginEmail");
            string? adminPassword = configuration["AdminPwd"] ?? Environment.GetEnvironmentVariable("AdminPwd");

            string? moderatorEmail = configuration["ModeratorLoginEmail"] ?? Environment.GetEnvironmentVariable("ModeratorLoginEmail");
            string? moderatorPassword = configuration["ModeratorPwd"] ?? Environment.GetEnvironmentVariable("ModeratorPwd");

            try
            {
                if (!string.IsNullOrEmpty(adminEmail))
                {
                    AppUser? adminUser = await userManager.FindByEmailAsync(adminEmail);

                    if (adminUser == null)
                    {
                        adminUser = new AppUser()
                        {
                            UserName = adminEmail,
                            Email = adminEmail,
                            FirstName = "Gelson",
                            LastName = "Hernandez",
                            EmailConfirmed = true,
                        };

                        await userManager.CreateAsync(adminUser, adminPassword!);
                        await userManager.AddToRoleAsync(adminUser, _adminRole!);
                    }
                }

                if (!string.IsNullOrEmpty(moderatorEmail))
                {
                    AppUser? modUser = await userManager.FindByEmailAsync(moderatorEmail);

                    if (modUser == null)
                    {
                        modUser = new AppUser()
                        {
                            UserName = moderatorEmail,
                            Email = moderatorEmail,
                            FirstName = "Moderator",
                            LastName = "Hernandez",
                            EmailConfirmed = true,
                        };

                        await userManager.CreateAsync(modUser, moderatorPassword!);
                        await userManager.AddToRoleAsync(modUser, _moderatorRole!);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("************* ERROR *************");
                Console.WriteLine("Error Seeding Default Blog Users.");
                Console.WriteLine(ex.Message);
                Console.WriteLine("*********************************");

                throw;
            }

        }
      
        // Demo Users Seed Method
        private static async Task SeedDemoUsersAsync(UserManager<AppUser> userManager, IConfiguration configuration)
        {
            string? demoLoginEmail = configuration["DemoLoginEmail"] ?? Environment.GetEnvironmentVariable("DemoLoginEmail");
            string? demoLoginPassword = configuration["DemoLoginPassword"] ?? Environment.GetEnvironmentVariable("DemoLoginPassword");

            AppUser demoUser = new AppUser()
            {
                UserName = demoLoginEmail,
                Email = demoLoginEmail,
                FirstName = "Demo",
                LastName = "User",
                EmailConfirmed = true,
            };

            try
            {
                AppUser? appUser = await userManager.FindByEmailAsync(demoLoginEmail!);

                if (appUser == null)
                {
                    await userManager.CreateAsync(demoUser, demoLoginPassword!);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("************* ERROR *************");
                Console.WriteLine("Error Seeding Demo Login User.");
                Console.WriteLine(ex.Message);
                Console.WriteLine("*********************************");

                throw;
            }

        }
    }
}
