using JwtAuthApp.Services;

namespace JwtAuthApp.Models
{
    /// <summary>
    /// ViewModel para a página de geração de tokens
    /// </summary>
    public class TokenGeneratorViewModel
    {
        public Dictionary<string, string> AvailablePermissions { get; set; } = new();
        public PublicApiTokenRequest Request { get; set; } = new();
        public PublicApiTokenResponse? GeneratedToken { get; set; }
        public string? Success { get; set; }
        public string? Error { get; set; }
    }

    /// <summary>
    /// Request para validação de token via AJAX
    /// </summary>
    public class ValidateTokenRequest
    {
        public string Token { get; set; } = string.Empty;
    }
}