using DatingApp.Svc.DTOs;
using DatingApp.Svc.Entities;

namespace DatingApp.Svc.Interfaces;

public interface IUserRepository
{
  void Update(AppUser user);
  Task<IEnumerable<AppUser>> GetUsersAsync();
  Task<AppUser> GetUserByIdAsync(int id);
  Task<AppUser> GetUserByUserNameAsync(string userName);
  Task<IEnumerable<MemberDTO>> GetMembersAsync();
  Task<MemberDTO> GetMemberAsync(string userName);
  Task<bool> SaveAllAsync();
}