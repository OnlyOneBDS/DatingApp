using DatingApp.Svc.DTOs;
using DatingApp.Svc.Entities;
using DatingApp.Svc.Helpers;

namespace DatingApp.Svc.Interfaces;

public interface IMessageRepository
{
  Task<PagedList<MessageDTO>> GetMessagesForUser(MessageParams messageParams);
  Task<IEnumerable<MessageDTO>> GetMessageThread(string currentUserName, string recipientUserName);
  Task<Message> GetMessage(int id);
  void AddMessage(Message message);
  void DeleteMessage(Message message);
  Task<bool> SaveAllAsync();
}