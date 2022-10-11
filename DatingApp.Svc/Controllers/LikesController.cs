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
  private readonly IUnitOfWork unitOfWork;

  public LikesController(IUnitOfWork unitOfWork)
  {
    this.unitOfWork = unitOfWork;
  }

  [HttpGet]
  public async Task<ActionResult<IEnumerable<LikeDTO>>> GetUserLikes([FromQuery] LikesParams likesParams)
  {
    likesParams.UserId = User.GetUserId();
    var users = await unitOfWork.LikesRepository.GetUserLikes(likesParams);

    Response.AddPaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);

    return Ok(users);
  }

  [HttpPost("{userName}")]
  public async Task<ActionResult> AddLike(string userName)
  {
    var sourceUserId = User.GetUserId();
    var likedUser = await unitOfWork.UserRepository.GetUserByUserNameAsync(userName);
    var sourceUser = await unitOfWork.LikesRepository.GetUserWithLikes(sourceUserId);

    if (likedUser == null)
    {
      return NotFound();
    }

    if (sourceUser.UserName == userName)
    {
      return BadRequest("You cannot like yourself");
    }

    var userLike = await unitOfWork.LikesRepository.GetUserLike(sourceUserId, likedUser.Id);

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

    if (await unitOfWork.Complete())
    {
      return Ok();
    }

    return BadRequest("Failed to like user");
  }
}