using HRMSystem.Data;
using Microsoft.EntityFrameworkCore;
using HRMSystem.Models;
using Microsoft.AspNetCore.Identity;

namespace HRMSystem.Services
{
    public class DbSeeder
    {
        public static async Task SeedRolesAsync(AppDbContext context)
        {
            var requiredRoles = new List<string> { "Admin", "HR", "Manager", "Employee" };

            foreach (var roleName in requiredRoles)
            {
                var roleExists = await context.Roles.AnyAsync(r => r.RoleName == roleName);
                if (!roleExists)
                {
                    context.Roles.Add(new Role
                    {
                        RoleName = roleName,
                        CreatedAt = DateTime.UtcNow.AddHours(7)
                    });
                }
            }

            await context.SaveChangesAsync();
        }
        public static async Task SeedAdminAsync(AppDbContext context, IConfiguration config)
        {
            var email = config["AdminAccount:Email"];
            var password = config["AdminAccount:Password"];

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password)) return;

            var role = await context.Roles.FirstAsync(r => r.RoleName == "Admin");

            var exist = await context.Employees
                .Include(e => e.EmployeeRoles)
                .ThenInclude(er => er.Roles)
                .FirstOrDefaultAsync(e => e.Email == email && e.EmployeeRoles.Any(r => r.RoleId == role.Id));

            if (exist != null) return;

            var hasher = new PasswordHasher<Employee>();
            var admin = new Employee
            {
                FullName = "Admin",
                Email = email,
                HashPassword = hasher.HashPassword(null, password),
                Gender = "Default Admin Gender",
                Address = "Default Admin Address",
                Phone = "Default Admin Phone",
                CreatedAt = DateTime.UtcNow.AddHours(7),
                Status = EmployeeStatus.Active,
            };

            context.Employees.Add(admin);
            await context.SaveChangesAsync();

            context.EmployeeRoles.Add(new EmployeeRole
            {
                EmployeeId = admin.Id,
                RoleId = role.Id
            });

            await context.SaveChangesAsync();
        }

    }
}
