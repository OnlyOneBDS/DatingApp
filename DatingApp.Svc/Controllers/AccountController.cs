using System.Security.Cryptography;
using System.Text;
using DatingApp.Svc.Data;
using DatingApp.Svc.DTOs;
using DatingApp.Svc.Entities;
using DatingApp.Svc.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.Svc.Controllers;

public class AccountController : BaseController
{
  private readonly DatingDbContext context;
  private readonly ITokenService tokenService;

  public AccountController(DatingDbContext context, ITokenService tokenService)
  {
    this.context = context;
    this.tokenService = tokenService;
  }

  [HttpPost("register")]
  public async Task<ActionResult<UserDTO>> Register(RegisterDTO register)
  {
    if (await UserExists(register.UserName))
    {
      return BadRequest("UserName is taken");
    }

    using var hmac = new HMACSHA512();

    var user = new AppUser
    {
      UserName = register.UserName.ToLower(),
      PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(register.Password)),
      PasswordSalt = hmac.Key
    };

    context.Users.Add(user);
    await context.SaveChangesAsync();

    return new UserDTO
    {
      UserName = user.UserName,
      Token = tokenService.CreateToken(user)
    };
  }

  [HttpPost("login")]
  public async Task<ActionResult<UserDTO>> Login(LoginDTO login)
  {
    var user = await context.Users.Include(p => p.Photos).SingleOrDefaultAsync(u => u.UserName == login.UserName);

    if (user == null)
    {
      return Unauthorized("Invalid user");
    }

    using var hmac = new HMACSHA512(user.PasswordSalt);
    var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(login.Password));

    for (int i = 0; i < computedHash.Length; i++)
    {
      if (computedHash[i] != user.PasswordHash[i])
      {
        return Unauthorized("Invalid password");
      }
    }

    return new UserDTO
    {
      UserName = user.UserName,
      Token = tokenService.CreateToken(user),
      PhotoUrl = user.Photos.FirstOrDefault(p => p.IsMain)?.Url
    };
  }

  private async Task<bool> UserExists(string userName)
  {
    return await context.Users.AnyAsync(u => u.UserName == userName.ToLower());
  }
}