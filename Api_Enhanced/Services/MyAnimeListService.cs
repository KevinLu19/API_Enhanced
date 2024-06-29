using Api_Enhanced.Models;
using System.Net.Http;

namespace Api_Enhanced.Services;

public class MyAnimeListService
{
	private string? _mal_client_id = Environment.GetEnvironmentVariable("Restful_MAL_API_CLIENT_ID");
	private readonly HttpClient _http_client;
	private string _base_api_url = "https://api.myanimelist.net/v2/";

    public MyAnimeListService(HttpClient http_client)
    {
        _http_client = http_client;
    }

	// Fetches the data using MAL api and then is used for the controller.
    public async Task<Anime> GetAnimeByIdAsync(int anime_id)
	{
		// Set a header for upcoming api request.
		_http_client.DefaultRequestHeaders.Add("X-MAL-CLIENT-ID", _mal_client_id);

		// Make api call.
		var response = await _http_client.GetAsync($"https://api.myanimelist.net/v2/anime/{anime_id}?fields=id,title,synopsis,main_picture,mean");

		if (response.IsSuccessStatusCode)
		{
			var animeData = await response.Content.ReadFromJsonAsync<Anime>();
			
			return animeData;
		}
		else
		{
			throw new HttpRequestException($"Request failed with status code {response.StatusCode}");
		}

	}


}
