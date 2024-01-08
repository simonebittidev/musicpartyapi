using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace SpotifyAPIs.Controllers;

[ApiController]
[Route("[controller]")]
public class LoginController : ControllerBase
{
    private readonly ILogger<LoginController> _logger;

    public LoginController(ILogger<LoginController> logger)
    {
        _logger = logger;
    }

    [HttpGet("GetTokenAsync")]
    public async Task<string> GetTokenAsync(string clientId, string grantType, string code, string redirectUri, string codeVerifier)
    {
        var par = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("client_id", clientId),
            new KeyValuePair<string, string>("grant_type", grantType),
            new KeyValuePair<string, string>("code", code),
            new KeyValuePair<string, string>("redirect_uri", redirectUri),
            new KeyValuePair<string, string>("code_verifier", codeVerifier)
        };

        using (var httpClient = new HttpClient())
        {
            httpClient.BaseAddress = new Uri("https://accounts.spotify.com/");

            using (var content = new FormUrlEncodedContent(par))
            {
                content.Headers.Clear();
                content.Headers.Add("Content-Type", "application/x-www-form-urlencoded");

                HttpResponseMessage response = await httpClient.PostAsync("api/token", content);

                return await response.Content.ReadAsStringAsync();
            }
        }
    }

    [HttpGet("GetRedirectToAuthCodeFlowUrl")]
    public string GetRedirectToAuthCodeFlowUrl(string clientId, string verifier, string redirectUri)
    {
        var challenge = GenerateCodeChallenge(verifier);

        var stringBuilder = new StringBuilder();

        stringBuilder.Append($"client_id={clientId}&");
        stringBuilder.Append($"response_type=code&");
        stringBuilder.Append($"scope=user-read-private user-read-email&");
        stringBuilder.Append($"redirect_uri={redirectUri}&");
        stringBuilder.Append($"code_challenge_method=S256&");
        stringBuilder.Append($"code_challenge={challenge}");

        return $"https://accounts.spotify.com/authorize?{stringBuilder}";
    }

    [HttpGet("GenerateCodeVerifier")]
    public string GenerateCodeVerifier(int length)
    {
        string text = "";
        string possible = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        Random random = new Random();

        for (int i = 0; i < length; i++)
        {
            text += possible[random.Next(possible.Length)];
        }

        return text;
    }

    private string GenerateCodeChallenge(string codeVerifier)
    {
        using (var sha256 = System.Security.Cryptography.SHA256.Create())
        {
            var data = Encoding.UTF8.GetBytes(codeVerifier);
            var hash = sha256.ComputeHash(data);

            var base64 = Convert.ToBase64String(hash);
            base64 = base64.Replace('+', '-');
            base64 = base64.Replace('/', '_');
            base64 = base64.TrimEnd('=');

            return base64;
        }
    }
}