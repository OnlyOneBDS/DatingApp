using AutoMapper;
using DatingApp.Svc.DTOs;
using DatingApp.Svc.Entities;
using DatingApp.Svc.Extensions;

namespace DatingApp.Svc.Helpers;

public class AutoMapperProfiles : Profile
{
  public AutoMapperProfiles()
  {
    CreateMap<AppUser, MemberDTO>()
      .ForMember(dest => dest.PhotoUrl, options => options.MapFrom(src => src.Photos.FirstOrDefault(p => p.IsMain).Url))
      .ForMember(dest => dest.Age, options => options.MapFrom(src => src.DateOfBirth.CalculateAge()));
    CreateMap<Photo, PhotoDTO>();
    CreateMap<MemberUpdateDTO, AppUser>();
    CreateMap<RegisterDTO, AppUser>();
    CreateMap<Message, MessageDTO>()
      .ForMember(dest => dest.SenderPhotoUrl, options => options.MapFrom(src => src.Sender.Photos.FirstOrDefault(u => u.IsMain).Url))
      .ForMember(dest => dest.RecipientPhotoUrl, options => options.MapFrom(src => src.Recipient.Photos.FirstOrDefault(u => u.IsMain).Url));
    CreateMap<DateTime, DateTime>().ConvertUsing(dt => DateTime.SpecifyKind(dt, DateTimeKind.Utc));
  }
}