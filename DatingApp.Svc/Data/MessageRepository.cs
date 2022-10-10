using AutoMapper;
using AutoMapper.QueryableExtensions;
using DatingApp.Svc.DTOs;
using DatingApp.Svc.Entities;
using DatingApp.Svc.Helpers;
using DatingApp.Svc.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.Svc.Data;

public class MessageRepository : IMessageRepository
{
  private readonly DatingDbContext context;
  private readonly IMapper mapper;

  public MessageRepository(DatingDbContext context, IMapper mapper)
  {
    this.context = context;
    this.mapper = mapper;
  }

  public void AddMessage(Message message)
  {
    context.Messages.Add(message);
  }

  public void DeleteMessage(Message message)
  {
    context.Messages.Remove(message);
  }

  public async Task<Message> GetMessage(int id)
  {
    return await context.Messages
      .Include(u => u.Sender)
      .Include(u => u.Recipient
      ).SingleOrDefaultAsync(m => m.Id == id);
  }

  public async Task<PagedList<MessageDTO>> GetMessagesForUser(MessageParams messageParams)
  {
    var query = context.Messages.OrderByDescending(m => m.MessageSent).AsQueryable();

    query = messageParams.Container switch
    {
      "Inbox" => query.Where(u => u.Recipient.UserName == messageParams.UserName && u.RecipientDeleted == false),
      "Outbox" => query.Where(u => u.Sender.UserName == messageParams.UserName && u.SenderDeleted == false),
      _ => query.Where(u => u.Recipient.UserName == messageParams.UserName && u.RecipientDeleted == false && u.DateRead == null)
    };

    var messages = query.ProjectTo<MessageDTO>(mapper.ConfigurationProvider);

    return await PagedList<MessageDTO>.CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);
  }

  public async Task<IEnumerable<MessageDTO>> GetMessageThread(string currentUserName, string recipientUserName)
  {
    var messages = await context.Messages
      .Include(u => u.Sender).ThenInclude(p => p.Photos)
      .Include(u => u.Recipient).ThenInclude(p => p.Photos)
      .Where(m => m.RecipientUserName == currentUserName && m.Sender.UserName == recipientUserName && m.RecipientDeleted == false ||
                  m.RecipientUserName == recipientUserName && m.Sender.UserName == currentUserName && m.SenderDeleted == false)
      .OrderBy(m => m.MessageSent)
      .ToListAsync();

    var unreadMessages = messages.Where(m => m.DateRead == null && m.Recipient.UserName == currentUserName).ToList();

    if (unreadMessages.Any())
    {
      foreach (var message in unreadMessages)
      {
        message.DateRead = DateTime.Now;
      }

      await context.SaveChangesAsync();
    }

    return mapper.Map<IEnumerable<MessageDTO>>(messages);
  }

  public async Task<bool> SaveAllAsync()
  {
    return await context.SaveChangesAsync() > 0;
  }
}
