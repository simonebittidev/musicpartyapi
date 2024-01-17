using EasyCaching.Core;
using Microsoft.AspNetCore.Mvc;
using SpotifyAPI.Web;
using SpotifyAPIs.Entities;
using SpotifyAPIs.Model;
using SpotifyAPIs.Provider;
using static SpotifyAPI.Web.PlaylistRemoveItemsRequest;

namespace SpotifyAPIs.Controllers;

[ApiController]
[Route("[controller]")]
public class MusicController : ControllerBase
{
    private readonly ILogger<AuthController> _logger;
    private readonly FirestoreProvider _firestoreProvider;

    public MusicController(ILogger<AuthController> logger, FirestoreProvider firestoreProvider)
    {
        _logger = logger;
        _firestoreProvider = firestoreProvider;
    }

    [HttpGet("GetCurrentSong")]
    public async Task<ActionResult<FullTrack?>> GetCurrentSong(string userId)
    {
        try
        {

            var login = await _firestoreProvider.Get<Login>(userId, CancellationToken.None);

            if(!string.IsNullOrEmpty(login?.AccessToken))
            {
                var song = await new SpotifyClient(login.AccessToken).Player.GetCurrentlyPlaying(new PlayerCurrentlyPlayingRequest());

                if(song.Item is FullTrack track)
                    return new OkObjectResult(track);
            }

            return new NotFoundResult();

        }
        catch (Exception ex)
        {
            return new StatusCodeResult(500);
        }
    }

    [HttpGet("SearchMusic")]
    public async Task<ActionResult<SearchResponse?>> SearchMusic(string userId, string query, int? limit)
    {
        var login = await _firestoreProvider.Get<Login>(userId, CancellationToken.None);

        if (!string.IsNullOrEmpty(login?.AccessToken))
        {
            var searchClient = new SpotifyClient(login.AccessToken).Search;

            var request = new SearchRequest((SearchRequest.Types.Album | SearchRequest.Types.Track | SearchRequest.Types.Artist), query)
            {
                Limit = limit ?? 10,
            };

            var result = await searchClient.Item(request);

            return result;
        }

        return default;
    }
}
