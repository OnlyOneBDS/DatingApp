using DatingApp.Svc.DTOs;
using DatingApp.Svc.Entities;
using DatingApp.Svc.Helpers;

namespace DatingApp.Svc.Interfaces;

public interface IUserRepository
{
  void Update(AppUser user);
  Task<IEnumerable<AppUser>> GetUsersAsync();
  Task<AppUser> GetUserByIdAsync(int id);
  Task<AppUser> GetUserByUserNameAsync(string userName);
  Task<PagedList<MemberDTO>> GetMembersAsync(UserParams userParams);
  Task<MemberDTO> GetMemberAsync(string userName);
  Task<string> GetUserGender(string userName);
}