using AutoMapper;
using DatingApp.Svc.DTOs;
using DatingApp.Svc.Entities;
using DatingApp.Svc.Extensions;
using DatingApp.Svc.Helpers;
using DatingApp.Svc.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.Svc.Controllers;

[Authorize]
public class UsersController : BaseController
{
  private readonly IUnitOfWork unitOfWork;
  private readonly IMapper mapper;
  private readonly IPhotoService photoService;

  public UsersController(IUnitOfWork unitOfWork, IMapper mapper, IPhotoService photoService)
  {
    this.unitOfWork = unitOfWork;
    this.mapper = mapper;
    this.photoService = photoService;
  }

  [HttpGet]
  public async Task<ActionResult<IEnumerable<MemberDTO>>> GetUsers([FromQuery] UserParams userParams)
  {
    var gender = await unitOfWork.UserRepository.GetUserGender(User.GetUserName());

    userParams.CurrentUserName = User.GetUserName();

    if (string.IsNullOrEmpty(userParams.Gender))
    {
      userParams.Gender = gender == "male" ? "female" : "male";
    }

    var users = await unitOfWork.UserRepository.GetMembersAsync(userParams);

    Response.AddPaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);

    return Ok(users);
  }

  [HttpGet("{userName}", Name = "GetUser")]
  public async Task<ActionResult<MemberDTO>> GetUser(string userName)
  {
    return await unitOfWork.UserRepository.GetMemberAsync(userName);
  }

  [HttpPost("add-photo")]
  public async Task<ActionResult<PhotoDTO>> AddPhoto(IFormFile file)
  {
    var user = await unitOfWork.UserRepository.GetUserByUserNameAsync(User.GetUserName());
    var result = await photoService.AddPhotoAsync(file);

    if (result.Error != null)
    {
      return BadRequest(result.Error.Message);
    }

    var photo = new Photo
    {
      Url = result.SecureUrl.AbsoluteUri,
      PublicId = result.PublicId
    };

    if (user.Photos.Count == 0)
    {
      photo.IsMain = true;
    }

    user.Photos.Add(photo);

    if (await unitOfWork.Complete())
    {
      //return mapper.Map<PhotoDTO>(photo);
      //return CreatedAtRoute("GetUser", mapper.Map<PhotoDTO>(photo));
      return CreatedAtRoute("GetUser", new { userName = user.UserName }, mapper.Map<PhotoDTO>(photo));
    }

    return BadRequest("Problem adding photo");
  }

  [HttpPut]
  public async Task<ActionResult> UpdateUser(MemberUpdateDTO memberUpdate)
  {
    var user = await unitOfWork.UserRepository.GetUserByUserNameAsync(User.GetUserName());

    mapper.Map(memberUpdate, user);
    unitOfWork.UserRepository.Update(user);

    if (await unitOfWork.Complete())
    {
      return NoContent();
    }

    return BadRequest("Failed to update user");
  }

  [HttpPut("set-main-photo/{photoId}")]
  public async Task<ActionResult> SetMainPhoto(int photoId)
  {
    var user = await unitOfWork.UserRepository.GetUserByUserNameAsync(User.GetUserName());
    var photo = user.Photos.FirstOrDefault(p => p.Id == photoId);

    if (photo.IsMain)
    {
      return BadRequest("This is already your main photo");
    }

    var currentMain = user.Photos.FirstOrDefault(p => p.IsMain);

    if (currentMain != null)
    {
      currentMain.IsMain = false;
    }

    photo.IsMain = true;

    if (await unitOfWork.Complete())
    {
      return NoContent();
    }

    return BadRequest("Failed to set main photo");
  }

  [HttpDelete("delete-photo/{photoId}")]
  public async Task<ActionResult> DeletePhoto(int photoId)
  {
    var user = await unitOfWork.UserRepository.GetUserByUserNameAsync(User.GetUserName());
    var photo = user.Photos.FirstOrDefault(p => p.Id == photoId);

    if (photo == null)
    {
      return NotFound();
    }

    if (photo.IsMain)
    {
      return BadRequest("You cannot delete your main photo");
    }

    if (photo.PublicId != null)
    {
      var result = await photoService.DeletePhotoAsync(photo.PublicId);

      if (result.Error != null)
      {
        return BadRequest(result.Error.Message);
      }
    }

    user.Photos.Remove(photo);

    if (await unitOfWork.Complete())
    {
      return Ok();
    }

    return BadRequest("Failed to delete photo");
  }
}