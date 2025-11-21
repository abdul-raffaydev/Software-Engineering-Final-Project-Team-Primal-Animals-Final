using Microsoft.AspNetCore.Identity;

public class ApplicationUser : IdentityUser
{
    public int? AppUserId { get; set; }
}
