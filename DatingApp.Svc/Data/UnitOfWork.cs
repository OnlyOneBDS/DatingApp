using AutoMapper;
using DatingApp.Svc.Interfaces;

namespace DatingApp.Svc.Data;

public class UnitOfWork : IUnitOfWork
{
  private readonly DatingDbContext context;
  private readonly IMapper mapper;

  public UnitOfWork(DatingDbContext context, IMapper mapper)
  {
    this.context = context;
    this.mapper = mapper;
  }

  public IUserRepository UserRepository => new UserRepository(context, mapper);

  public IMessageRepository MessageRepository => new MessageRepository(context, mapper);

  public ILikesRepository LikesRepository => new LikesRepository(context);

  public async Task<bool> Complete()
  {
    return await context.SaveChangesAsync() > 0;
  }

  public bool HasChanges()
  {
    return context.ChangeTracker.HasChanges();
  }
}
