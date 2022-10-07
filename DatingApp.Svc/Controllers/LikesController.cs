using DatingApp.Svc.DTOs;
using DatingApp.Svc.Entities;
using DatingApp.Svc.Extensions;
using DatingApp.Svc.Helpers;
using DatingApp.Svc.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.Svc.Controllers;

[Authorize]
public class LikesController : BaseController
{
  private readonly IUserRepository userRepository;
  private readonly ILikesRepository likesRepository;

  public LikesController(IUserRepository userRepository, ILikesRepository likesRepository)
  {
    this.userRepository = userRepository;
    this.likesRepository = likesRepository;
  }


  [HttpGet]
  public async Task<ActionResult<IEnumerable<LikeDTO>>> GetUserLikes([FromQuery] LikesParams likesParams)
  {
    likesParams.UserId = User.GetUserId();
    var users = await likesRepository.GetUserLikes(likesParams);

    Response.AddPaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);

    return Ok(users);
  }

  [HttpPost("{userName}")]
  public async Task<ActionResult> AddLike(string userName)
  {
    var sourceUserId = User.GetUserId();
    var likedUser = await userRepository.GetUserByUserNameAsync(userName);
    var sourceUser = await likesRepository.GetUserWithLikes(sourceUserId);

    if (likedUser == null)
    {
      return NotFound();
    }

    if (sourceUser.UserName == userName)
    {
      return BadRequest("You cannot like yourself");
    }

    var userLike = await likesRepository.GetUserLike(sourceUserId, likedUser.Id);

    if (userLike != null)
    {
      return BadRequest("You already like this user");
    }

    userLike = new UserLike
    {
      SourceUserId = sourceUserId,
      LikedUserId = likedUser.Id
    };

    sourceUser.LikedUsers.Add(userLike);

    if (await userRepository.SaveAllAsync())
    {
      return Ok();
    }

    return BadRequest("Failed to like user");
  }
}