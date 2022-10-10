using AutoMapper;
using DatingApp.Svc.DTOs;
using DatingApp.Svc.Entities;
using DatingApp.Svc.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.Svc.Controllers;

public class AccountController : BaseController
{
  private readonly UserManager<AppUser> userManager;
  private readonly SignInManager<AppUser> signInManager;
  private readonly ITokenService tokenService;
  private readonly IMapper mapper;

  public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ITokenService tokenService, IMapper mapper)
  {
    this.userManager = userManager;
    this.signInManager = signInManager;
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

    user.UserName = register.UserName.ToLower();

    var result = await userManager.CreateAsync(user, register.Password);

    if (!result.Succeeded)
    {
      return BadRequest(result.Errors);
    }

    var roleResult = await userManager.AddToRoleAsync(user, "Member");

    if (!roleResult.Succeeded)
    {
      return BadRequest(roleResult.Errors);
    }

    return new UserDTO
    {
      UserName = user.UserName,
      Token = await tokenService.CreateToken(user),
      KnownAs = user.KnownAs,
      Gender = user.Gender
    };
  }

  [HttpPost("login")]
  public async Task<ActionResult<UserDTO>> Login(LoginDTO login)
  {
    var user = await userManager.Users
      .Include(p => p.Photos)
      .SingleOrDefaultAsync(u => u.UserName == login.UserName);

    if (user == null)
    {
      return Unauthorized("Invalid user");
    }

    var result = await signInManager.CheckPasswordSignInAsync(user, login.Password, false);

    if (!result.Succeeded)
    {
      return Unauthorized();
    }

    return new UserDTO
    {
      UserName = user.UserName,
      Token = await tokenService.CreateToken(user),
      PhotoUrl = user.Photos.FirstOrDefault(p => p.IsMain)?.Url,
      KnownAs = user.KnownAs,
      Gender = user.Gender
    };
  }

  private async Task<bool> UserExists(string userName)
  {
    return await userManager.Users.AnyAsync(u => u.UserName == userName.ToLower());
  }
}