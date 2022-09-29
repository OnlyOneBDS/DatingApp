using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DatingApp.Svc.Entities;
using DatingApp.Svc.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace DatingApp.Svc.Services;

public class TokenService : ITokenService
{
  private readonly SymmetricSecurityKey key;

  public TokenService(IConfiguration config)
  {
    key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["TokenKey"]));
  }

  public string CreateToken(AppUser user)
  {
    var claims = new List<Claim>
    {
      new Claim(JwtRegisteredClaimNames.NameId, user.UserName)
    };

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
