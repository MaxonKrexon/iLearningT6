#nullable disable

using Microsoft.AspNetCore.Identity;

namespace webapptask4.Areas.Identity.Data;

public class ApplicationUser : IdentityUser
{
    public DateTime? RegistrationDate {get; set;}

    public DateTime? LastLogin {get; set;}
}