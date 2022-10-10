using Microsoft.AspNetCore.Identity;

namespace DatingApp.Svc.Entities;

public class AppRole : IdentityRole<int>
{
  public ICollection<AppUserRole> UserRoles { get; set; }
}
