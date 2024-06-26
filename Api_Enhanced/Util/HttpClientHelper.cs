namespace Api_Enhanced.Util;

public static class HttpClientHelper
{
	private static HttpClient _httpClient = new();

	public static async Task<string> GetAsync(string url)
	{
		HttpResponseMessage response = await _httpClient.GetAsync(url);
		response.EnsureSuccessStatusCode();

		return await response.Content.ReadAsStringAsync();
	}
}
