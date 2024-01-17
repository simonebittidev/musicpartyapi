using Microsoft.AspNetCore.Mvc;
using SpotifyAPIs.Entities;
using SpotifyAPIs.Provider;

namespace SpotifyAPIs.Controllers;

[ApiController]
[Route("[controller]")]
public class PartyQueueController : ControllerBase
{
    private readonly ILogger<AuthController> _logger;
    private readonly FirestoreProvider _firestoreProvider;

    public PartyQueueController(ILogger<AuthController> logger, FirestoreProvider firestoreProvider)
    {
        _logger = logger;
        _firestoreProvider = firestoreProvider;
    }

    [HttpGet("GetPartyQueuesByUserId")]
    public async Task<ActionResult<IReadOnlyCollection<PartyQueue>>> GetPartyQueuesByUserId(string userId)
    {
        try
        {
            if (string.IsNullOrEmpty(userId))
                return new BadRequestResult();

            var queues = await _firestoreProvider.WhereArrayContains<PartyQueue>(nameof(PartyQueue.UsersId), userId, CancellationToken.None);

            return new OkObjectResult(queues);
        }
        catch (Exception ex)
        {
            return new StatusCodeResult(500);
        }
    }

    [HttpGet("CreateNewPartyQueue")]
    public async Task<ActionResult<PartyQueue>> CreateNewPartyQueue(string userId, string partyName, string partyDescription)
    {
        try
        {
            if (string.IsNullOrEmpty(userId))
                return new BadRequestResult();

            if (string.IsNullOrEmpty(partyName))
                return new BadRequestResult();

            var partyQueue = new PartyQueue(userId, partyName, partyDescription);
            await _firestoreProvider.AddOrUpdate<PartyQueue>(partyQueue, CancellationToken.None);

            return new OkObjectResult(partyQueue);
        }
        catch (Exception ex)
        {
            return new StatusCodeResult(500);
        }
    }

    [HttpGet("DeleteParty")]
    public async Task<ActionResult> DeleteParty(string partyId)
    {
        try
        {
            if (string.IsNullOrEmpty(partyId))
                return new BadRequestResult();

            await _firestoreProvider.Delete<PartyQueue>(partyId, CancellationToken.None);

            return Ok();
        }
        catch (Exception ex)
        {
            return new StatusCodeResult(500);
        }
    }

    [HttpGet("JoinParty")]
    public async Task<ActionResult> JoinParty(string userId, string partyId)
    {
        try
        {
            if (string.IsNullOrEmpty(userId))
                return new BadRequestResult();

            if (string.IsNullOrEmpty(partyId))
                return new BadRequestResult();

            var party = await _firestoreProvider.Get<PartyQueue>(partyId, CancellationToken.None);

            if (!party.UsersId.Contains(userId))
            {
                party.UsersId.Append(userId);
                await _firestoreProvider.AddOrUpdate<PartyQueue>(party, CancellationToken.None);
            }

            return new OkResult();
        }
        catch (Exception ex)
        {
            return new StatusCodeResult(500);
        }
    }
}
