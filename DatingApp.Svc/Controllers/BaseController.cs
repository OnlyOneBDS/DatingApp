using DatingApp.Svc.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.Svc.Controllers;

[ApiController]
[Route("api/[controller]")]
[ServiceFilter(typeof(LogUserActivity))]
public class BaseController : ControllerBase { }