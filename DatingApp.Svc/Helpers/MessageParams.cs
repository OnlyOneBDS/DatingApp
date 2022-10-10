namespace DatingApp.Svc.Helpers;

public class MessageParams : PaginationParams
{
  public string UserName { get; set; }
  public string Container { get; set; } = "Unread";
}
