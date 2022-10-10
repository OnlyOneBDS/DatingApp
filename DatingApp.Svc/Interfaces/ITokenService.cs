using DatingApp.Svc.Entities;

namespace DatingApp.Svc.Interfaces;

public interface ITokenService
{
  Task<string> CreateToken(AppUser user);
}
