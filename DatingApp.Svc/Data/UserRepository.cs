using AutoMapper;
using AutoMapper.QueryableExtensions;
using DatingApp.Svc.DTOs;
using DatingApp.Svc.Entities;
using DatingApp.Svc.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.Svc.Data;

public class UserRepository : IUserRepository
{
  private readonly DatingDbContext context;
  private readonly IMapper mapper;

  public UserRepository(DatingDbContext context, IMapper mapper)
  {
    this.context = context;
    this.mapper = mapper;
  }

  public async Task<MemberDTO> GetMemberAsync(string userName)
  {
    return await context.Users
      .Where(u => u.UserName == userName)
      .ProjectTo<MemberDTO>(mapper.ConfigurationProvider)
      .SingleOrDefaultAsync();
  }

  public async Task<IEnumerable<MemberDTO>> GetMembersAsync()
  {
    return await context.Users
      .ProjectTo<MemberDTO>(mapper.ConfigurationProvider)
      .ToListAsync();
  }

  public async Task<AppUser> GetUserByIdAsync(int id)
  {
    return await context.Users.FindAsync(id);
  }

  public async Task<AppUser> GetUserByUserNameAsync(string userName)
  {
    return await context.Users.Include(p => p.Photos).SingleOrDefaultAsync(u => u.UserName == userName);
  }

  public async Task<IEnumerable<AppUser>> GetUsersAsync()
  {
    return await context.Users.Include(p => p.Photos).ToListAsync();
  }

  public async Task<bool> SaveAllAsync()
  {
    return await context.SaveChangesAsync() > 0;
  }

  public void Update(AppUser user)
  {
    context.Entry(user).State = EntityState.Modified;
  }
}