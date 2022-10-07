using DatingApp.Svc.DTOs;
using DatingApp.Svc.Entities;
using DatingApp.Svc.Extensions;
using DatingApp.Svc.Helpers;
using DatingApp.Svc.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.Svc.Data;

public class LikesRepository : ILikesRepository
{
  private readonly DatingDbContext context;

  public LikesRepository(DatingDbContext context)
  {
    this.context = context;
  }

  public async Task<UserLike> GetUserLike(int sourceUserId, int likedUserId)
  {
    return await context.Likes.FindAsync(sourceUserId, likedUserId);
  }

  public async Task<PagedList<LikeDTO>> GetUserLikes(LikesParams likesParams)
  {
    var users = context.Users.OrderBy(u => u.UserName).AsQueryable();
    var likes = context.Likes.AsQueryable();

    if (likesParams.Predicate == "liked")
    {
      likes = likes.Where(like => like.SourceUserId == likesParams.UserId);
      users = likes.Select(like => like.LikedUser);
    }

    if (likesParams.Predicate == "likedBy")
    {
      likes = likes.Where(like => like.LikedUserId == likesParams.UserId);
      users = likes.Select(like => like.SourceUser);
    }

    var likedUsers = users.Select(user => new LikeDTO
    {
      Id = user.Id,
      UserName = user.UserName,
      KnownAs = user.KnownAs,
      Age = user.DateOfBirth.CalculateAge(),
      PhotoUrl = user.Photos.FirstOrDefault(p => p.IsMain).Url,
      City = user.City
    });

    return await PagedList<LikeDTO>.CreateAsync(likedUsers, likesParams.PageNumber, likesParams.PageSize);
  }

  public async Task<AppUser> GetUserWithLikes(int userId)
  {
    return await context.Users.Include(l => l.LikedUsers).FirstOrDefaultAsync(u => u.Id == userId);
  }
}