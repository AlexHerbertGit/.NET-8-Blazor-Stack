namespace KobraKai.API.Dtos
{
    public record MealCreateDto(string Title, string? Description, string[]? DietaryTags, int PortionsAvailable);
    public record MealDto(Guid Id, string Title, string? Description, string[] DietaryTags, int PortionsAvailable, string ProviderId);
}
