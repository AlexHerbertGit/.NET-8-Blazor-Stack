using System;

namespace KobraKai.API.Dtos
{
    public sealed class MealDto
    {
        public Guid Id { get; init; }
        public string Title { get; init; } = "";
        public string? Description { get; init; }
        public string[] DietaryTags { get; init; } = Array.Empty<string>();
        public int PortionsAvailable { get; init; }
        public string? ProviderId { get; init; }
    }

    public sealed class MealCreateDto
    {
        public string Title { get; init; } = "";
        public string? Description { get; init; }
        public string[] DietaryTags { get; init; } = Array.Empty<string>();
        public int PortionsAvailable { get; init; }
    }

    public sealed class MealUpdateDto
    {
        public string Title { get; init; } = "";
        public string? Description { get; init; }
        public string[] DietaryTags { get; init; } = Array.Empty<string>();
        public int PortionsAvailable { get; init; }
    }
}

