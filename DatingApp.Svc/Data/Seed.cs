using System.Text.Json;
using DatingApp.Svc.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.Svc.Data;

public class Seed
{
  public static async Task SeedUsers(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
  {
    if (await userManager.Users.AnyAsync())
    {
      return;
    }

    var userData = await File.ReadAllTextAsync("Data/UserSeedData.json");
    var users = JsonSerializer.Deserialize<List<AppUser>>(userData);

    if (users == null)
    {
      return;
    }

    var roles = new List<AppRole>
    {
      new AppRole { Name = "Admin" },
      new AppRole { Name = "Moderator" },
      new AppRole { Name = "Member" },
    };

    foreach (var role in roles)
    {
      await roleManager.CreateAsync(role);
    }

    foreach (var user in users)
    {
      user.UserName = user.UserName.ToLower();

      await userManager.CreateAsync(user, "Pa$$w0rd");
      await userManager.AddToRoleAsync(user, "Member");
    }

    var admin = new AppUser { UserName = "admin" };

    await userManager.CreateAsync(admin, "Pa$$w0rd");
    await userManager.AddToRolesAsync(admin, new[] { "Admin", "Moderator" });
  }
}