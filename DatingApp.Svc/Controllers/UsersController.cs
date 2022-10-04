﻿using AutoMapper;
using DatingApp.Svc.DTOs;
using DatingApp.Svc.Entities;
using DatingApp.Svc.Extensions;
using DatingApp.Svc.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.Svc.Controllers;

[Authorize]
public class UsersController : BaseController
{
  private readonly IUserRepository userRepository;
  private readonly IMapper mapper;
  private readonly IPhotoService photoService;

  public UsersController(IUserRepository userRepository, IMapper mapper, IPhotoService photoService)
  {
    this.userRepository = userRepository;
    this.mapper = mapper;
    this.photoService = photoService;
  }

  [HttpGet]
  public async Task<ActionResult<IEnumerable<MemberDTO>>> GetUsers()
  {
    var users = await userRepository.GetMembersAsync();

    return Ok(users);
  }

  [HttpGet("{userName}", Name = "GetUser")]
  public async Task<ActionResult<MemberDTO>> GetUser(string userName)
  {
    return await userRepository.GetMemberAsync(userName);
  }

  [HttpPost("add-photo")]
  public async Task<ActionResult<PhotoDTO>> AddPhoto(IFormFile file)
  {
    var user = await userRepository.GetUserByUserNameAsync(User.GetUserName());
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

    if (await userRepository.SaveAllAsync())
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
    var user = await userRepository.GetUserByUserNameAsync(User.GetUserName());

    mapper.Map(memberUpdate, user);
    userRepository.Update(user);

    if (await userRepository.SaveAllAsync())
    {
      return NoContent();
    }

    return BadRequest("Failed to update user");
  }

  [HttpPut("set-main-photo/{photoId}")]
  public async Task<ActionResult> SetMainPhoto(int photoId)
  {
    var user = await userRepository.GetUserByUserNameAsync(User.GetUserName());
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

    if (await userRepository.SaveAllAsync())
    {
      return NoContent();
    }

    return BadRequest("Failed to set main photo");
  }

  [HttpDelete("delete-photo/{photoId}")]
  public async Task<ActionResult> DeletePhoto(int photoId)
  {
    var user = await userRepository.GetUserByUserNameAsync(User.GetUserName());
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

    if (await userRepository.SaveAllAsync())
    {
      return Ok();
    }

    return BadRequest("Failed to delete photo");
  }
}