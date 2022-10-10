﻿using System.Text;
using DatingApp.Svc.Data;
using DatingApp.Svc.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace DatingApp.Svc.Extensions;

public static class IdentityServiceExtensions
{
  public static IServiceCollection AddIdentityServices(this IServiceCollection services, IConfiguration config)
  {
    services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
      .AddJwtBearer(options =>
      {
        options.TokenValidationParameters = new TokenValidationParameters
        {
          IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["TokenKey"])),
          ValidateIssuer = false,
          ValidateAudience = false,
          ValidateIssuerSigningKey = true
        };
      });

    services.AddIdentityCore<AppUser>(options => { options.Password.RequireNonAlphanumeric = false; })
      .AddRoles<AppRole>()
      .AddRoleManager<RoleManager<AppRole>>()
      .AddSignInManager<SignInManager<AppUser>>()
      .AddRoleValidator<RoleValidator<AppRole>>()
      .AddEntityFrameworkStores<DatingDbContext>();

    services.AddAuthorization(options =>
    {
      options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
      options.AddPolicy("ModeratePhotoRole", policy => policy.RequireRole("Admin", "Moderator"));
    });

    return services;
  }
}
