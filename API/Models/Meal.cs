

namespace KobraKai.API.Models
{
    public class Meal
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = default!;
        public string? Description { get; set; }
        public string DietaryTagsCsv { get; set; } = "";
        public int PortionsAvailable { get; set; } = 0; // Defaultm starting value

        public string ProviderId { get; set; } = default!;
        public ApplicationUser? Provider { get; set; }
    }
}
