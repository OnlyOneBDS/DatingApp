using AutoMapper;
using AutoMapper.QueryableExtensions;
using DatingApp.Svc.DTOs;
using DatingApp.Svc.Entities;
using DatingApp.Svc.Helpers;
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

  public async Task<PagedList<MemberDTO>> GetMembersAsync(UserParams userParams)
  {
    var query = context.Users.AsQueryable();

    query = query.Where(u => u.UserName != userParams.CurrentUserName);
    query = query.Where(u => u.Gender == userParams.Gender);

    var minDOB = DateTime.Today.AddYears(-userParams.MaxAge - 1);
    var maxDOB = DateTime.Today.AddYears(-userParams.MinAge);

    query = query.Where(u => u.DateOfBirth >= minDOB && u.DateOfBirth <= maxDOB);

    query = userParams.OrderBy switch
    {
      "created" => query.OrderByDescending(u => u.Created),
      _ => query.OrderByDescending(u => u.LastActive)
    };

    return await PagedList<MemberDTO>
      .CreateAsync(query.ProjectTo<MemberDTO>(mapper.ConfigurationProvider).AsNoTracking(), userParams.PageNumber, userParams.PageSize);
  }

  public async Task<AppUser> GetUserByIdAsync(int id)
  {
    return await context.Users.FindAsync(id);
  }

  public async Task<AppUser> GetUserByUserNameAsync(string userName)
  {
    return await context.Users.Include(p => p.Photos).SingleOrDefaultAsync(u => u.UserName == userName);
  }

  public async Task<string> GetUserGender(string userName)
  {
    return await context.Users
      .Where(u => u.UserName == userName)
      .Select(u => u.Gender)
      .FirstOrDefaultAsync();
  }

  public async Task<IEnumerable<AppUser>> GetUsersAsync()
  {
    return await context.Users.Include(p => p.Photos).ToListAsync();
  }

  public void Update(AppUser user)
  {
    context.Entry(user).State = EntityState.Modified;
  }
}