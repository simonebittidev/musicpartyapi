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
    private readonly IConfiguration _configuration;


    public AuthController(ILogger<AuthController> logger, FirestoreProvider firestoreProvider, IConfiguration configuration)
    {
        _logger = logger;
        _firestoreProvider = firestoreProvider;
        _configuration = configuration;
    }

    [HttpGet("GenerateToken")]
    public async Task<string> GenerateTokenAsync(string code, string redirectUri)
    {
        var initialResponse = await new OAuthClient().RequestToken(
            new PKCETokenRequest(_configuration.GetValue<string>("Spotify:ClientId", "")!, code, new Uri(redirectUri), _configuration.GetValue<string>("Spotify:Verifier", "")!)
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
    public string GetRedirectToAuthCodeFlowUrl(string redirectUri)
    {
        var challenge = GenerateChallenge();

        var loginRequest = new LoginRequest(
              new Uri(redirectUri),
              _configuration.GetValue<string>("Spotify:ClientId", "")!,
              LoginRequest.ResponseType.Code
            )
        {
            CodeChallengeMethod = "S256",
            CodeChallenge = challenge,
            Scope = new[] { Scopes.PlaylistReadPrivate, Scopes.PlaylistReadCollaborative, Scopes.UserReadCurrentlyPlaying, Scopes.UserReadPrivate, Scopes.AppRemoteControl, Scopes. }
        };
        var uri = loginRequest.ToUri();

        return uri.ToString();
    }

    private string GenerateChallenge()
    {
        var (verifier, challenge) = PKCEUtil.GenerateCodes("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789");

        return challenge;
    }
}
