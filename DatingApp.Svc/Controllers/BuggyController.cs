using DatingApp.Svc.Data;
using DatingApp.Svc.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.Svc.Controllers;

public class BuggyController : BaseController
{
  private readonly DatingDbContext context;

  public BuggyController(DatingDbContext context)
  {
    this.context = context;
  }

  [Authorize]
  [HttpGet("auth")]
  public ActionResult<string> GetSecret()
  {
    return "secret text";
  }

  [HttpGet("not-found")]
  public ActionResult<AppUser> GetNotFound()
  {
    var thing = context.Users.Find(-1);

    if (thing == null)
    {
      return NotFound();
    }

    return Ok(thing);
  }

  [HttpGet("server-error")]
  public ActionResult<string> GetServerError()
  {
    var thing = context.Users.Find(-1);
    var thingToReturn = thing.ToString();

    return thingToReturn;
  }

  [HttpGet("bad-request")]
  public ActionResult<string> GetBadRequest()
  {
    return BadRequest("This was not a good request");
  }
}