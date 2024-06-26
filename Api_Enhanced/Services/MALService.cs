using Api_Enhanced.Util;

namespace Api_Enhanced.Services;

/*
 Save to database:
 List of top recognizable studios (List is expandable):
	8bit, Bones,Production I.G,Cloverworks,Dogo Kobo, Wit Studio, Madhouse, Kyoto Animation, A-1 Pictures

 GET list of all current airing anime
		-> From that, sort by top studios from above. Within that, sort by highest MAL score.

 */

public class MALService
{
	// obtain from environmental variable.
	private readonly string? _CLIENT_ID = Environment.GetEnvironmentVariable("Restful_MAL_API_CLIENT_ID");
	private readonly string _BASE_URL = "https://api.myanimelist.net/v2/";



	// Fetches anime information that I deemed important and removes all the other filterings.
	// Generic anime id search
	public async Task<string> FetchAnimeDetail(string anime_id)
	{
		string url = $"{_BASE_URL}/anime/{anime_id}?fields=id,title,main_picture,start_date,end_date,synopsis,mean,rank,popularity,media_type,status,genres,num_episodes,start_season,broadcast,source,recommendations,studios,statistics";

		return await HttpClientHelper.GetAsync(url);
	}

	public async Task<string> FetchCurrentSeason(int year, string season)
	{
		string url = $"{_BASE_URL}/anime/season/{year}/{season}";

		return await HttpClientHelper.GetAsync(url);
	}



	public void Test()
	{
		Console.WriteLine($"The value of Client ID is: {_CLIENT_ID}");
		Console.WriteLine($"base url is: {_BASE_URL}");
	}
}