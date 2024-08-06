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

	/*
	 Example actress names for testing:
     Tanezaki, Atsumi
	 Hanazawa, Kana
     */

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
}
