using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hive_IT.Data
{
    public class DBContextSeedData
    {
        private ApplicationDataContext _context;

        public DBContextSeedData(ApplicationDataContext context)
        {
            _context = context;
        }

        public async void SeedAdminUser()
        {
            //default user information
            var admin = new ApplicationUser
            {
                FirstName = "Default",
                LastName= "Admin",
                UserName = "defaultuser",
                NormalizedUserName = "DEFAULTUSER",
                Email = "email@email.com",
                NormalizedEmail = "EMAIL@EMAIL.COM",
                PhoneNumber = "5555551234",
                EmailConfirmed =true,
                PhoneNumberConfirmed = true,
                DateCreated= DateTime.Now,
                LockoutEnabled = false,
                SecurityStamp= Guid.NewGuid().ToString()
            };

            var roleStore = new RoleStore<IdentityRole>(_context);

            //check if role of Admin exists, if not create it
            if (!_context.Roles.Any(r => r.Name == "Admin"))
            {
                await roleStore.CreateAsync(new IdentityRole { Name = "Admin", NormalizedName="ADMIN"});
            }

            // same for manager
            if (!_context.Roles.Any(r => r.Name == "Manager"))
            {
                await roleStore.CreateAsync(new IdentityRole { Name = "Manager" , NormalizedName="MANAGER"});
            }

            //find the Id for the Admin role
            var adminRole = _context.Roles.First(r => r.Name == "Admin");
            var adminId = adminRole.Id;

            //if no UserRoles that have the ID for Admin exist then no users have role of Admin,
            //check for default user existance as well, just for security
            if (!_context.UserRoles.Any(r => r.RoleId == adminId) && !_context.Users.Any(u => u.UserName == admin.UserName))
            {
                //so create a default
                var hasher = new PasswordHasher<ApplicationUser>();
                var hashedPass = hasher.HashPassword(admin, "password");
                admin.PasswordHash = hashedPass;
                var userStore = new UserStore<ApplicationUser>(_context);
                await userStore.CreateAsync(admin);
            }            
            
            await _context.SaveChangesAsync();

        }
    }
}
