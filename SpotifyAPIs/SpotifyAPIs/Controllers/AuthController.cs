using Microsoft.AspNetCore.Mvc;
using SpotifyAPI.Web;
using SpotifyAPIs.Entities;
using SpotifyAPIs.Provider;

namespace SpotifyAPIs.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly ILogger<AuthController> _logger;
    private readonly FirestoreProvider _firestoreProvider;

    public AuthController(ILogger<AuthController> logger, FirestoreProvider firestoreProvider)
    {
        _logger = logger;
        _firestoreProvider = firestoreProvider;
    }

    [HttpGet("GenerateToken")]
    public async Task<string> GenerateTokenAsync(string clientId, string code, string redirectUri, string verifier)
    {
        var initialResponse = await new OAuthClient().RequestToken(
            new PKCETokenRequest(clientId, code, new Uri(redirectUri), verifier)
        );

        //TODO: try to deserialize JWT to get userid instead of execute a call to UserProfile
        var user = await new SpotifyClient(initialResponse.AccessToken).UserProfile.Current();

        if(!string.IsNullOrEmpty(user?.Id))
        {
            var newLogin = new Login(user.Id, initialResponse.AccessToken);
            await _firestoreProvider.AddOrUpdate<Login>(newLogin, CancellationToken.None);
        }

        return user.Id; //Return userId to client 
    }

    [HttpGet("GetUserInfo")]
    public async Task<PrivateUser> GetUserInfo(string userId)
    {
        var userLogin = await _firestoreProvider.Get<Login>(userId, CancellationToken.None);

        var user = await new SpotifyClient(userLogin.AccessToken).UserProfile.Current();

        return user; //Return userId to client 
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
            Scope = new[] { Scopes.PlaylistReadPrivate, Scopes.PlaylistReadCollaborative, Scopes.UserReadCurrentlyPlaying, Scopes.UserReadPrivate}
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
