using DatingApp.Svc.DTOs;
using DatingApp.Svc.Entities;
using DatingApp.Svc.Helpers;

namespace DatingApp.Svc.Interfaces;

public interface IMessageRepository
{
  Task<PagedList<MessageDTO>> GetMessagesForUser(MessageParams messageParams);
  Task<IEnumerable<MessageDTO>> GetMessageThread(string currentUserName, string recipientUserName);
  Task<Message> GetMessage(int id);
  Task<Connection> GetConnection(string connectionId);
  Task<Group> GetMessageGroup(string groupName);
  Task<Group> GetGroupForConnection(string connectionId);
  void AddGroup(Group group);
  void RemoveConnection(Connection connection);
  void AddMessage(Message message);
  void DeleteMessage(Message message);
}