using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

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

    //params.append("client_id", clientId);
    //params.append("grant_type", "authorization_code");
    //params.append("code", code);
    //params.append("redirect_uri", "http://localhost:5173/callback");
    //params.append("code_verifier", verifier!);

    [HttpGet(Name = "Get")]
    public async Task<string> GetAsync(string clientId, string grantType, string code, string redirectUri, string codeVerifier, string clientSecret)
    {
        var par = new List<KeyValuePair<string, string>>();
        par.Add(new KeyValuePair<string, string>("client_id", clientId));
        par.Add(new KeyValuePair<string, string>("grant_type", grantType));
        par.Add(new KeyValuePair<string, string>("client_secret", clientSecret));

        //par.Add("code", code);
        //par.Add("redirect_uri", redirectUri);
        //par.Add("code_verifier", codeVerifier);

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


}



