using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Google.Cloud.Firestore.V1;
using Microsoft.AspNetCore.Builder.Extensions;
using Microsoft.AspNetCore.Mvc;
using SpotifyAPI.Web;
using SpotifyAPIs.Entities;
using SpotifyAPIs.Provider;
using static Google.Cloud.Firestore.V1.StructuredQuery.Types;

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

        var user = await new SpotifyClient(initialResponse.AccessToken).UserProfile.Current();
        if(!string.IsNullOrEmpty(user?.Id))
        {
            var newLogin = new Login(user.Id, initialResponse.AccessToken);
            await _firestoreProvider.AddOrUpdate<Login>(newLogin, CancellationToken.None);
        }

        return initialResponse.AccessToken; //Return tge 
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
