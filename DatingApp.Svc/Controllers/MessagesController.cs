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
public class MessagesController : BaseController
{
  private readonly IUserRepository userRepository;
  private readonly IMessageRepository messageRepository;
  private readonly IMapper mapper;

  public MessagesController(IUserRepository userRepository, IMessageRepository messageRepository, IMapper mapper)
  {
    this.userRepository = userRepository;
    this.messageRepository = messageRepository;
    this.mapper = mapper;
  }

  [HttpGet]
  public async Task<ActionResult<IEnumerable<MessageDTO>>> GetMessagesForUser([FromQuery] MessageParams messageParams)
  {
    messageParams.UserName = User.GetUserName();

    var messages = await messageRepository.GetMessagesForUser(messageParams);

    Response.AddPaginationHeader(messages.CurrentPage, messages.PageSize, messages.TotalCount, messages.TotalPages);

    return messages;
  }

  [HttpGet("thread/{userName}")]
  public async Task<ActionResult<IEnumerable<MessageDTO>>> GetMessageThread(string userName)
  {
    var currentUserName = User.GetUserName();

    return Ok(await messageRepository.GetMessageThread(currentUserName, userName));
  }


  [HttpPost]
  public async Task<ActionResult<MessageDTO>> CreateMessage(CreateMessageDTO createMessageDTO)
  {
    var userName = User.GetUserName();

    if (userName == createMessageDTO.RecipientUserName.ToLower())
    {
      return BadRequest("You cannot send messages to yourself");
    }

    var sender = await userRepository.GetUserByUserNameAsync(userName);
    var recipient = await userRepository.GetUserByUserNameAsync(createMessageDTO.RecipientUserName);

    if (recipient == null)
    {
      return NotFound();
    }

    var message = new Message
    {
      Sender = sender,
      SenderUserName = sender.UserName,
      Recipient = recipient,
      RecipientUserName = recipient.UserName,
      Content = createMessageDTO.Content
    };

    messageRepository.AddMessage(message);

    if (await messageRepository.SaveAllAsync())
    {
      return Ok(mapper.Map<MessageDTO>(message));
    }

    return BadRequest("Failed to send message");
  }

  [HttpDelete("{id}")]
  public async Task<ActionResult> DeleteMessage(int id)
  {
    var userName = User.GetUserName();
    var message = await messageRepository.GetMessage(id);

    if (message.Sender.UserName != userName && message.Recipient.UserName != userName)
    {
      return Unauthorized();
    }

    if (message.Sender.UserName == userName)
    {
      message.SenderDeleted = true;
    }

    if (message.Recipient.UserName == userName)
    {
      message.RecipientDeleted = true;
    }

    if (message.SenderDeleted && message.RecipientDeleted)
    {
      messageRepository.DeleteMessage(message);
    }

    if (await messageRepository.SaveAllAsync())
    {
      return Ok();
    }

    return BadRequest("Problem deleting the message");
  }
}
