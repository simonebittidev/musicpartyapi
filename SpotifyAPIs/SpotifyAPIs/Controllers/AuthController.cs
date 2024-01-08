using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SpotifyAPI.Web;

namespace SpotifyAPIs.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly ILogger<AuthController> _logger;

    public AuthController(ILogger<AuthController> logger)
    {
        _logger = logger;
    }

    [HttpGet("GenerateToken")]
    public async Task<string> GenerateTokenAsync(string clientId, string code, string redirectUri, string verifier)
    {
        var initialResponse = await new OAuthClient().RequestToken(
            new PKCETokenRequest(clientId, code, new Uri(redirectUri), verifier)
        );

        return initialResponse.AccessToken; //return user id and save token to the storage
    }

    [HttpGet("GetRedirectToAuthCodeFlowUrl")]
    public string GetRedirectToAuthCodeFlowUrl(string clientId, string challenge, string redirectUri)
    {
        var loginRequest = new LoginRequest(
              new Uri(redirectUri),
              clientId,
              LoginRequest.ResponseType.Code
            )
        {
            CodeChallengeMethod = "S256",
            CodeChallenge = challenge,
            Scope = new[] { Scopes.PlaylistReadPrivate, Scopes.PlaylistReadCollaborative }
        };
        var uri = loginRequest.ToUri();

        return uri.ToString();
    }

    [HttpGet("GenerateCodeVerifier")]
    public Dictionary<string,string> GenerateCodes()
    {
        var (verifier, challenge) = PKCEUtil.GenerateCodes("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789");

        var dictionary = new Dictionary<string, string>
        {
            { "verifier", verifier },
            { "challenge", challenge }
        };

        return dictionary;
    }
}


public class Token
{
    [JsonProperty("access_token")]
    public string AccessToken { get; set; }

    [JsonProperty("token_type")]
    public string TokenType { get; set; }

    [JsonProperty("express_in")]
    public int ExpiresIn { get; set; }
}
