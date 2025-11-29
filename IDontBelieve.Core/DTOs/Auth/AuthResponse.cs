using IDontBelieve.Core.Models;

namespace IDontBelieve.Core.DTOs.Auth;

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public DateTime Expiration { get; set; }
    public User User { get; set; } = new();
}