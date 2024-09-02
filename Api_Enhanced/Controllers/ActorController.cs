using Api_Enhanced.Models;
using Api_Enhanced.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;


namespace Api_Enhanced.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ActorController : Controller
{
	private readonly MALActor _mal_actor = new MALActor();

	// Endpoint: api/people/<name>
	// Sort result by most favorited characters -> display anime name + character's name together.
	[HttpGet("{name}")]
	public async Task<ActionResult<List<string>>> GetPeopleRoles(string name)
	{
		if (string.IsNullOrEmpty(name))
			return BadRequest("Actor name is required");

		var fetch_info_result = await _mal_actor.FetchPeopleInfo(name);

		if (fetch_info_result == null || fetch_info_result.Count == 0)
		{
			return NotFound("Actor/ Actress not found.");
		}

		return Ok(fetch_info_result);
	}

	// Endpoint: api/people/id
	// retervies detailed information about a specific actor/actress by ID. Information includes but not limited to bio, career highlight,etc.

	// Endpoint: api/people/popularitiy 
	// Provides metrics on the actor/actress’s popularity, such as the number of followers, mentions, or search trends. Another metric could be member favorites on MAL
	[HttpGet("{name}/popularity")]
	public async Task<ActionResult<List<string>>> GetActorPopularity(string name)
	{
		if (string.IsNullOrEmpty(name))
			return BadRequest("Actor name is required.");

		var popularity_string = await _mal_actor.GetActorPopularity(name);

		if (popularity_string == null)
			return NotFound("Popularity does not exist/ something went wrong.");

		return Ok(popularity_string);
	}
}
