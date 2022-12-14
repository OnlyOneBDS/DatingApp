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

  public void AddGroup(Group group)
  {
    context.Groups.Add(group);
  }

  public void AddMessage(Message message)
  {
    context.Messages.Add(message);
  }

  public void DeleteMessage(Message message)
  {
    context.Messages.Remove(message);
  }

  public async Task<Connection> GetConnection(string connectionId)
  {
    return await context.Connections.FindAsync(connectionId);
  }

  public async Task<Group> GetGroupForConnection(string connectionId)
  {
    return await context.Groups
    .Include(c => c.Connections)
    .Where(c => c.Connections.Any(x => x.ConnectionId == connectionId))
    .FirstOrDefaultAsync();
  }

  public async Task<Message> GetMessage(int id)
  {
    return await context.Messages
      .Include(u => u.Sender)
      .Include(u => u.Recipient
      ).SingleOrDefaultAsync(m => m.Id == id);
  }

  public async Task<Group> GetMessageGroup(string groupName)
  {
    return await context.Groups.Include(c => c.Connections).FirstOrDefaultAsync(g => g.Name == groupName);
  }

  public async Task<PagedList<MessageDTO>> GetMessagesForUser(MessageParams messageParams)
  {
    var query = context.Messages
      .OrderByDescending(m => m.MessageSent)
      .ProjectTo<MessageDTO>(mapper.ConfigurationProvider)
      .AsQueryable();

    query = messageParams.Container switch
    {
      "Inbox" => query.Where(u => u.RecipientUserName == messageParams.UserName && u.RecipientDeleted == false),
      "Outbox" => query.Where(u => u.SenderUserName == messageParams.UserName && u.SenderDeleted == false),
      _ => query.Where(u => u.RecipientUserName == messageParams.UserName && u.RecipientDeleted == false && u.DateRead == null)
    };

    return await PagedList<MessageDTO>.CreateAsync(query, messageParams.PageNumber, messageParams.PageSize);
  }

  public async Task<IEnumerable<MessageDTO>> GetMessageThread(string currentUserName, string recipientUserName)
  {
    var messages = await context.Messages
      .Where(m => m.RecipientUserName == currentUserName && m.Sender.UserName == recipientUserName && m.RecipientDeleted == false ||
                  m.RecipientUserName == recipientUserName && m.Sender.UserName == currentUserName && m.SenderDeleted == false)
      .OrderBy(m => m.MessageSent)
      .ProjectTo<MessageDTO>(mapper.ConfigurationProvider)
      .ToListAsync();

    var unreadMessages = messages.Where(m => m.DateRead == null && m.RecipientUserName == currentUserName).ToList();

    if (unreadMessages.Any())
    {
      foreach (var message in unreadMessages)
      {
        message.DateRead = DateTime.UtcNow;
      }
    }

    return messages;
  }

  public void RemoveConnection(Connection connection)
  {
    context.Connections.Remove(connection);
  }
}
