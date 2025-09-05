namespace KobraKai.API.Dtos
{
    public record RegisterRequest(string Name, string Email, string Password, string Role, string? Address);
    public record LoginRequest(string Email, string Password);
    public record MeResponse(string Id, string Email, string Role, int TokenBalance, string? Name);
    
}
