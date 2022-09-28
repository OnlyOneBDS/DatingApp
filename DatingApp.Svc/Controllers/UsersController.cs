using DatingApp.Svc.Data;
using DatingApp.Svc.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.Svc.Controllers;

public class UsersController : BaseController
{
  private readonly DatingDbContext context;

  public UsersController(DatingDbContext context)
  {
    this.context = context;
  }

  [HttpGet]
  public async Task<ActionResult<IEnumerable<AppUser>>> GetUsers()
  {
    var users = await context.Users.ToListAsync();

    return users;
  }

  [HttpGet("{id}")]
  public async Task<ActionResult<AppUser>> GetUser(int id)
  {
    var user = await context.Users.FindAsync(id);

    return user;
  }
}