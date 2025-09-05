namespace KobraKai.API.Dtos
{
    public record PlaceOrderDto(Guid MealId);
    public record OrderDto(Guid Id, string BeneficiaryId, string MemberId, Guid MealId, string Status, DateTime OrderedAt);
}
