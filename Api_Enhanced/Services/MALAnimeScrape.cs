using Api_Enhanced.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using System.Text.RegularExpressions;

namespace Api_Enhanced.Services;

public interface IMALAnimeScrape
{
	Task<Anime> GetReview(int anime_id);
	Task<List<string>> CurrentSeason();
	Task<List<string>> GetAnimeNews();
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
		var firefox_options = new FirefoxOptions();
		firefox_options.AddArgument("-headless");

		var firefox_default_service = FirefoxDriverService.CreateDefaultService();

		_driver = new FirefoxDriver(firefox_default_service, firefox_options);
	}

	// Endpoint: api/anime/{id}/review
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


	// Endpoint: api/anime/current season
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

	// For api/anime/top_anime

	// Endpoint: api/anime/news
	public async Task<List<string>> GetAnimeNews()
	{
		string news_url = "https://www.animenewsnetwork.com/";

		_driver.Navigate().GoToUrl(news_url);

		// Select filter if not already selected. Filter for news, anime, manga
		var anime_topic = _driver.FindElement(By.XPath("//span[text()='Anime']"));
		var manga_topic = _driver.FindElement(By.XPath("//span[text()='Manga']"));
		var news_topic = _driver.FindElement(By.XPath("//span/span[text()='News']"));

		var selected_class = _driver.FindElement(By.XPath("//span[@class='selected']"));

		try
		{
			anime_topic.Click();
			manga_topic.Click();
			news_topic.Click();

			Console.WriteLine("Clicked on all 3 options.");
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.Message);
		}

		List<string> result = GetNewsPerDay();

		return result;
	}

	public List<string> GetNewsPerDay()
	{
		var news_day = _driver.FindElements(By.XPath("//div[@class='mainfeed-day']")).ToList();

		List<string> string_news_day = new List<string>();

		// Convert IWebElement to a list of strings.
		foreach (var item in news_day)
		{
			string_news_day.Add(item.Text.ToString());
		}

		return CleanTheList(string_news_day);
	}

	public List<string> CleanTheList(List<string> old_list)
	{
		// define pattern to match
		string pattern = @"\bNEWS\b|\b\d+ comments\b|\b\d{1,2}:\d{2}\b|\b[a-z]+ \b|\b\d+ comment\b";

		// Clean the list of strings
		List<string> cleaned_list = new List<string>();

		foreach (var text in old_list)
		{
			// replace matched patterns with empty string.
			string clean_text = Regex.Replace(text, pattern, "").Trim();

			cleaned_list.Add(clean_text);
		}


		return cleaned_list;
	}

	public void Dispose()
	{
		Thread.Sleep(2000);
		_driver.Quit();
	}
}
