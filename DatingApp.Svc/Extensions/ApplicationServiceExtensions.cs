using DatingApp.Svc.Data;
using DatingApp.Svc.Interfaces;
using DatingApp.Svc.Services;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.Svc.Extensions;

public static class ApplicationServiceExtensions
{
  public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
  {
    services.AddScoped<ITokenService, TokenService>();

    services.AddDbContext<DatingDbContext>(options =>
    {
      options.UseSqlServer(config.GetConnectionString("DefaultConnection"));
    });

    return services;
  }
}
