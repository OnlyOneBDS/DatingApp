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
  private readonly IUnitOfWork unitOfWork;
  private readonly IMapper mapper;

  public MessagesController(IUnitOfWork unitOfWork, IMapper mapper)
  {
    this.unitOfWork = unitOfWork;
    this.mapper = mapper;
  }

  [HttpGet]
  public async Task<ActionResult<IEnumerable<MessageDTO>>> GetMessagesForUser([FromQuery] MessageParams messageParams)
  {
    messageParams.UserName = User.GetUserName();

    var messages = await unitOfWork.MessageRepository.GetMessagesForUser(messageParams);

    Response.AddPaginationHeader(messages.CurrentPage, messages.PageSize, messages.TotalCount, messages.TotalPages);

    return messages;
  }

  [HttpPost]
  public async Task<ActionResult<MessageDTO>> CreateMessage(CreateMessageDTO createMessageDTO)
  {
    var userName = User.GetUserName();

    if (userName == createMessageDTO.RecipientUserName.ToLower())
    {
      return BadRequest("You cannot send messages to yourself");
    }

    var sender = await unitOfWork.UserRepository.GetUserByUserNameAsync(userName);
    var recipient = await unitOfWork.UserRepository.GetUserByUserNameAsync(createMessageDTO.RecipientUserName);

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

    unitOfWork.MessageRepository.AddMessage(message);

    if (await unitOfWork.Complete())
    {
      return Ok(mapper.Map<MessageDTO>(message));
    }

    return BadRequest("Failed to send message");
  }

  [HttpDelete("{id}")]
  public async Task<ActionResult> DeleteMessage(int id)
  {
    var userName = User.GetUserName();
    var message = await unitOfWork.MessageRepository.GetMessage(id);

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
      unitOfWork.MessageRepository.DeleteMessage(message);
    }

    if (await unitOfWork.Complete())
    {
      return Ok();
    }

    return BadRequest("Problem deleting the message");
  }
}
