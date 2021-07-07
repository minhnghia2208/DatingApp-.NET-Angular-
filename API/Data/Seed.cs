using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using API.Entity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class Seed
    {
        public static async Task SeedUser(UserManager<AppUser> userManager
            , RoleManager<AppRole> roleManager){
            if (await userManager.Users.AnyAsync()) return;
            var userData = await System.IO.File.ReadAllTextAsync("Data/UserDataSeed.json");
            var users = JsonSerializer.Deserialize<List<AppUser>>(userData);

            if (users == null) return;
            var roles = new List<AppRole>{
                new AppRole{Name = "Member"},
                new AppRole{Name = "Admin"},
                new AppRole{Name = "Moderator"},
            };
            foreach (var role in roles){
                await roleManager.CreateAsync(role);
            }
            foreach (var user in users)
            {
                user.UserName = user.UserName.ToLower();
                await userManager.CreateAsync(user, "password");
                await userManager.AddToRoleAsync(user, "Member");
            }
            var admin = new AppUser{
                UserName = "admin"
            };
            await userManager.CreateAsync(admin, "password");
            await userManager.AddToRolesAsync(admin, new[] {
                "Admin", "Moderator"
            });
        }
    }
}