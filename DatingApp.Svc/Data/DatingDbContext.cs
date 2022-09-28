using DatingApp.Svc.Entities;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.Svc.Data
{
  public class DatingDbContext : DbContext
  {
    public DatingDbContext(DbContextOptions<DatingDbContext> options) : base(options) { }

    public DbSet<AppUser> Users { get; set; }
  }
}