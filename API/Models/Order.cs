namespace KobraKai.API.Models
{
    public class Order
    {
        public Guid Id { get; set; }
        public string BenficiaryId { get; set; } = default!;
        public string MemberId { get; set; } = default!;
        public Guid MealId { get; set; }
        public string Status { get; set; } = "Pending"; // Pending | Accepted | Delivered | Cancelled.

        public Meal? Meal { get; set; }
    }
}
