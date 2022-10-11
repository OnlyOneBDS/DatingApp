using DatingApp.Svc.Data;
using DatingApp.Svc.Helpers;
using DatingApp.Svc.Interfaces;
using DatingApp.Svc.Services;
using DatingApp.Svc.SignalR;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.Svc.Extensions;

public static class ApplicationServiceExtensions
{
  public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
  {
    services.AddAutoMapper(typeof(AutoMapperProfiles).Assembly);

    services.AddDbContext<DatingDbContext>(options =>
    {
      options.UseSqlServer(config.GetConnectionString("DefaultConnection"));
    });

    services.AddScoped<LogUserActivity>();

    services.AddScoped<IUnitOfWork, UnitOfWork>();

    services.AddScoped<IPhotoService, PhotoService>();
    services.AddScoped<ITokenService, TokenService>();

    services.AddSingleton<PresenceTracker>();

    services.Configure<CloudinarySettings>(config.GetSection("CloudinarySettings"));

    return services;
  }
}
