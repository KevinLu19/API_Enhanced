using Api_Enhanced.Models;
using Api_Enhanced.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;


namespace Api_Enhanced.Controllers;

/*
 Save to database:
 List of top recognizable studios (List is expandable):
	8bit, Bones,Production I.G,Cloverworks,Dogo Kobo, Wit Studio, Madhouse, Kyoto Animation, A-1 Pictures

 GET list of all current airing anime
		-> From that, sort by top studios from above. Within that, sort by highest MAL score.

 */

[Route("api/[controller]")]
[ApiController]
public class AnimeController : Controller
{
	private readonly MyAnimeListService _anime_service;
    private readonly IMALAnimeScrape _anime_scrape;

    public AnimeController(MyAnimeListService anime_service, IMALAnimeScrape anime_scrape)
    {
        _anime_service = anime_service;
        _anime_scrape = anime_scrape;
    }

    // Endpoint: api/anime/{id}
    [HttpGet("{anime_id}")]
    public async Task<ActionResult<Anime>> GetAnimeById(int anime_id)
    {
        try
        {
            var anime = await _anime_service.GetAnimeByIdAsync(anime_id);

            return Ok(anime);
        }
        catch (HttpRequestException e) 
        {
            return StatusCode(500, e.Message);
        }
    }

    // Endpoint: api/anime/{id}/review
    [HttpGet("{anime_id}/reviews")]
    public async Task<ActionResult<Anime>> GetAnimeReview(int anime_id)
    {
        try
        {
			var anime_review = await _anime_scrape.GetReview(anime_id);

            return Ok(anime_review);
		}
        catch (HttpRequestException e) 
        {
            return StatusCode(500, e.Message);
        }
	}


    // Endpoint: api/anime/current_season
    [HttpGet("current_season")]
    public async Task<ActionResult<List<string>>> CurrentSeason()
    {
        try
        {
            var current_animes = await _anime_scrape.CurrentSeason();

            return Ok(current_animes);
        }
        catch (HttpRequestException e)
        {
            return StatusCode(500, e.Message);
        }
    }
}
