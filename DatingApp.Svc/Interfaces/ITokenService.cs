using DatingApp.Svc.Entities;

namespace DatingApp.Svc.Interfaces;

public interface ITokenService
{
  string CreateToken(AppUser user);
}
