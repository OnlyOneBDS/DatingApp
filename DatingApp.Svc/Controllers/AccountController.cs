using System.Security.Cryptography;
using System.Text;
using AutoMapper;
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
  private readonly IMapper mapper;

  public AccountController(DatingDbContext context, ITokenService tokenService, IMapper mapper)
  {
    this.context = context;
    this.tokenService = tokenService;
    this.mapper = mapper;
  }

  [HttpPost("register")]
  public async Task<ActionResult<UserDTO>> Register(RegisterDTO register)
  {
    if (await UserExists(register.UserName))
    {
      return BadRequest("UserName is taken");
    }

    var user = mapper.Map<AppUser>(register);

    using var hmac = new HMACSHA512();

    user.UserName = register.UserName.ToLower();
    user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(register.Password));
    user.PasswordSalt = hmac.Key;

    context.Users.Add(user);
    await context.SaveChangesAsync();

    return new UserDTO
    {
      UserName = user.UserName,
      Token = tokenService.CreateToken(user),
      KnownAs = user.KnownAs
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
      PhotoUrl = user.Photos.FirstOrDefault(p => p.IsMain)?.Url,
      KnownAs = user.KnownAs
    };
  }

  private async Task<bool> UserExists(string userName)
  {
    return await context.Users.AnyAsync(u => u.UserName == userName.ToLower());
  }
}