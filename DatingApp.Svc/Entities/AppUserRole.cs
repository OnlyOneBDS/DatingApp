using Microsoft.AspNetCore.Identity;

namespace DatingApp.Svc.Entities;

public class AppUserRole : IdentityUserRole<int>
{
  public AppUser User { get; set; }
  public AppRole Role { get; set; }
}
