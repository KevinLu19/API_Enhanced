using Api_Enhanced.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;

namespace Api_Enhanced.Services;

public interface IMALAnimeScrape
{
	Task<Anime> GetReview(int anime_id);
	Task<List<string>> CurrentSeason();
}

/*
 Scraping generalized items for the website for anime only.
 */
public class MALAnimeScrape : IDisposable, IMALAnimeScrape
{
    private IWebDriver _driver;
	private string _base_url = "https://myanimelist.net/anime";


	public MALAnimeScrape()
    {
		var firefox_default_service = FirefoxDriverService.CreateDefaultService();

		_driver = new FirefoxDriver(firefox_default_service);

		var firefox_options = new FirefoxOptions();
		firefox_options.AddArgument("headless");
	}

	// For api/anime/{id}/review
	public async Task<Anime> GetReview(int anime_id)
	{
		// Review: <div class = 'review-element js-review-element'>
		var full_url = $"{_base_url}/{anime_id}";

		List<string> list_anime_reviews = new List<string>();

		_driver.Navigate().GoToUrl(full_url);
		Thread.Sleep(2000);

		var find_anime_reviews = _driver.FindElements(By.XPath("//div[@class='review-element js-review-element']"));

		foreach (var item in find_anime_reviews)
		{
			list_anime_reviews.Add(item.Text);
        }

		Anime anime = new Anime()
		{
			Review = list_anime_reviews
		};

		return anime;
	}


	// For api/anime/current season
	public async Task<List<string>> CurrentSeason()
	{
		var url = "https://myanimelist.net/anime/season";

		// Limit to size 15 from myanimelist website sorted by score and grab the first 15 animes.
		List<string> list_current_anime = new List<string>();
		List<string> limit_list_size = new List<string>();

		_driver.Navigate().GoToUrl(url);
		Thread.Sleep(2000);

		// Find TV button
		var tv_btn = _driver.FindElement(By.XPath("//li[@class='btn-type js-btn-seasonal']"));

		if (tv_btn.Text == "TV")
		{
			tv_btn.Click();

			// Try to click on sort button
			try
			{
				var anime_title = _driver.FindElements(By.XPath("//h2[@class='h2_anime_title']"));

				foreach (var title in anime_title)
				{
					list_current_anime.Add(title.Text);
				}

				// Limit the size down to 15 for list using LINQ.
				limit_list_size = list_current_anime.Take(15).ToList();
			}
			catch (Exception ex) 
			{
                await Console.Out.WriteLineAsync(ex.Message);
            }
		}

		return limit_list_size;
	}

	public void Dispose()
	{
		Thread.Sleep(2000);
		_driver.Quit();
	}
}
