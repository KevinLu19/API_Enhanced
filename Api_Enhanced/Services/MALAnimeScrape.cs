using Api_Enhanced.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using System.Runtime;

namespace Api_Enhanced.Services;

/*
 Scraping generalized items for the website for anime only.
 */
public class MALAnimeScrape : IDisposable
{
    private IWebDriver _driver;
	private string _base_url = "https://myanimelist.net/anime";


	public MALAnimeScrape()
    {
		var firefox_default_service = FirefoxDriverService.CreateDefaultService();

		_driver = new FirefoxDriver(firefox_default_service);
	}

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

	public void Dispose()
	{
		Thread.Sleep(2000);
		_driver.Quit();
	}
}
