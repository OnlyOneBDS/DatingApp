using DatingApp.Svc.DTOs;
using DatingApp.Svc.Entities;
using DatingApp.Svc.Helpers;

namespace DatingApp.Svc.Interfaces;

public interface ILikesRepository
{
  Task<UserLike> GetUserLike(int sourceUserId, int likedUserId);
  Task<AppUser> GetUserWithLikes(int userId);
  Task<PagedList<LikeDTO>> GetUserLikes(LikesParams likesParams);
}