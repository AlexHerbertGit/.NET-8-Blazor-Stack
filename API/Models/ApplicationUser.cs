using Microsoft.AspNetCore.Identity;

namespace KobraKai.API.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Simple role column for prototype purposes
        public string Role { get; set; } = "beneficiary"; // "beneficiary" || "member".
        public string TokenBalance { get; set; } = 10; // Default for all starting beneficiary accounts.
    }
}
