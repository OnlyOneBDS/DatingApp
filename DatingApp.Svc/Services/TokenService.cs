using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DatingApp.Svc.Entities;
using DatingApp.Svc.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace DatingApp.Svc.Services;

public class TokenService : ITokenService
{
  private readonly SymmetricSecurityKey key;
  private readonly UserManager<AppUser> userManager;

  public TokenService(IConfiguration config, UserManager<AppUser> userManager)
  {
    key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["TokenKey"]));
    this.userManager = userManager;
  }

  public async Task<string> CreateToken(AppUser user)
  {
    var claims = new List<Claim>
    {
      new Claim(JwtRegisteredClaimNames.NameId, user.Id.ToString()),
      new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),
    };

    var roles = await userManager.GetRolesAsync(user);

    claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

    var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

    var tokenDescriptor = new SecurityTokenDescriptor
    {
      Expires = DateTime.Now.AddDays(7),
      SigningCredentials = credentials,
      Subject = new ClaimsIdentity(claims)
    };

    var tokenHandler = new JwtSecurityTokenHandler();
    var token = tokenHandler.CreateToken(tokenDescriptor);

    return tokenHandler.WriteToken(token);
  }
}
