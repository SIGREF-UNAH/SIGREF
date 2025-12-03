namespace SIGREF.API.Audit.Models;

public class LoginAuditDto
{
    public string UserId { get; set; }
    public string UserName { get; set; }
    public bool Success { get; set; }
    public string ErrorMessage { get; set; }
}
