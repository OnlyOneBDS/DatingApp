using DatingApp.Svc.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.Svc.Controllers;

public class AdminController : BaseController
{
  private readonly UserManager<AppUser> userManager;

  public AdminController(UserManager<AppUser> userManager)
  {
    this.userManager = userManager;
  }

  [Authorize(Policy = "RequireAdminRole")]
  [HttpGet("users-with-roles")]
  public async Task<ActionResult> GetUsersWithRoles()
  {
    var users = await userManager.Users
      .Include(r => r.UserRoles)
      .ThenInclude(r => r.Role)
      .OrderBy(u => u.UserName)
      .Select(u => new
      {
        u.Id,
        u.UserName,
        Roles = u.UserRoles.Select(r => r.Role.Name).ToList()
      })
      .ToListAsync();

    return Ok(users);
  }

  [HttpPost("edit-roles/{userName}")]
  public async Task<ActionResult> EditRole(string userName, [FromQuery] string roles)
  {
    var selectedRoles = roles.Split(",").ToArray();
    var user = await userManager.FindByNameAsync(userName);

    if (user == null)
    {
      return NotFound("Could not find user");
    }

    var userRoles = await userManager.GetRolesAsync(user);
    var result = await userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));

    if (!result.Succeeded)
    {
      return BadRequest("Failed to add to roles");
    }

    result = await userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));

    if (!result.Succeeded)
    {
      return BadRequest("Failed to remove from roles");
    }

    return Ok(await userManager.GetRolesAsync(user));
  }

  [Authorize(Policy = "ModeratePhotoRole")]
  [HttpGet("photos-to-moderate")]
  public ActionResult GetPhotosForModeration()
  {
    return Ok("Admins or Moderators can see this");
  }
}
