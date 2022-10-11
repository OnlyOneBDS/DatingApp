using DatingApp.Svc.Extensions;
using DatingApp.Svc.Interfaces;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DatingApp.Svc.Helpers;

public class LogUserActivity : IAsyncActionFilter
{
  public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
  {
    var resultContext = await next();

    if (!resultContext.HttpContext.User.Identity.IsAuthenticated)
    {
      return;
    }

    var userId = resultContext.HttpContext.User.GetUserId();
    var unitOfWork = resultContext.HttpContext.RequestServices.GetService<IUnitOfWork>();
    var user = await unitOfWork.UserRepository.GetUserByIdAsync(userId);

    user.LastActive = DateTime.UtcNow;

    await unitOfWork.Complete();
  }
}