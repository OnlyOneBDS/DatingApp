using Microsoft.AspNetCore.Mvc;

namespace DatingApp.Svc.Controllers;

public class FallbackController : Controller
{
  public ActionResult Index()
  {
    return PhysicalFile(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "index.html"), "text/html");
  }
}
