using Api_Enhanced.Models;
using Api_Enhanced.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;


namespace Api_Enhanced.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ActorController : Controller
{
	private readonly MALActor _actor;

    public ActorController(MALActor actor)
    {
        _actor = actor;
    }

    // Endpoint: api/people/<name>
    // Sort result by most favorited characters -> display anime name + character's name together.
    [HttpGet("{name}")]
	public async Task<ActionResult<Actor>> GetPeopleRoles(string name)
    {
        if (string.IsNullOrEmpty(name))
            return BadRequest("Actor name is required");

        Actor actor = await _actor.FetchPeopleInfo(name);

        if (actor == null)
        {
            return NotFound("Actor/ Actress not found.");
        }

        return Ok(actor);
	}
}
