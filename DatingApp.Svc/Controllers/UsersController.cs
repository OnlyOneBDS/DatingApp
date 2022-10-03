using AutoMapper;
using DatingApp.Svc.DTOs;
using DatingApp.Svc.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.Svc.Controllers;

[Authorize]
public class UsersController : BaseController
{
  private readonly IUserRepository userRepository;
  private readonly IMapper mapper;

  public UsersController(IUserRepository userRepository, IMapper mapper)
  {
    this.userRepository = userRepository;
    this.mapper = mapper;
  }

  [HttpGet]
  public async Task<ActionResult<IEnumerable<MemberDTO>>> GetUsers()
  {
    var users = await userRepository.GetMembersAsync();

    return Ok(users);
  }

  [HttpGet("{userName}")]
  public async Task<ActionResult<MemberDTO>> GetUser(string userName)
  {
    return await userRepository.GetMemberAsync(userName);
  }
}