using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;
using System.Text.RegularExpressions;

namespace Test_Api_Enhanced;
public class TestAnimeScrape : IWebScrape, IDisposable
{
    private IWebDriver _driver;
    private string _base_url = "https://myanimelist.net/anime";
	private readonly ITestOutputHelper _test_output;

	public TestAnimeScrape(ITestOutputHelper output)
    {
        var firefox_service = FirefoxDriverService.CreateDefaultService();
        _driver = new FirefoxDriver(firefox_service);

        _test_output = output;
    }

    public void TraverseUrl(string incomplete_url)
	{
        var url = $"{_base_url}/{incomplete_url}";

        _driver.Navigate().GoToUrl(url);
        Thread.Sleep(2000);
	}


    [Fact]
    public void GetAnimeReview()
    {
		// Example incomplete_url= 55791
		// 55791 ==> Oshi no Ko season 2
		TraverseUrl("55791");

		var find_anime_reviews = _driver.FindElements(By.XPath("//div[@class='review-element js-review-element']"));

		foreach (var item in find_anime_reviews)
		{
			_test_output.WriteLine(item.Text);
            _test_output.WriteLine("-------------");
		}
	}

    [Fact]
    public void CurrentSeason()
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
				
				foreach (var item in limit_list_size)
				{
					_test_output.WriteLine(item);
				}
			}
			catch (Exception ex)
			{
				_test_output.WriteLine(ex.Message);
			}
		}
	}

	// Endpoint: api/anime/news
	[Fact]
	public void TestGetAnimeNews()
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

			_test_output.WriteLine("Clicked on all 3 options.");

			GetNewsPerDay();
		}
		catch (Exception ex)
		{
			_test_output.WriteLine(ex.Message);
		}
	}

	public void GetNewsPerDay()
	{
		Dictionary<string, string> news_feed = new Dictionary<string, string>();

		var news_day = _driver.FindElements(By.XPath("//div[@class='mainfeed-day']")).ToList();

		List<string> string_news_day = new List<string>();

		// Convert IWebElement to a list of strings.
		foreach (var item in news_day)
		{
			string_news_day.Add(item.Text.ToString());
		}

		// Print list<string> converted from above.	
		//foreach (var item in string_news_day)
		//{
		//	item.Replace("NEWS", "");
		//}

		CleanTheList(string_news_day);
	}

	public void CleanTheList(List<string> old_list)
	{
		// define pattern to match
		string pattern = @"\bNEWS\b|\b\d+ comments\b|\b\d{1,3}:\d{2}\b|\b[a-z]+ \b";

		// Clean the list of strings
		List<string> cleaned_list = new List<string>();

		foreach (var text in old_list)
		{
			// replace matched patterns with empty string.
			string clean_text = Regex.Replace(text, pattern, "").Trim();

			cleaned_list.Add(clean_text);
		}

		// print
		foreach (string item in cleaned_list)
		{
			_test_output.WriteLine(item);
		}
	}

	public void Dispose()
    {
        Thread.Sleep(2000);
        _driver.Quit();
    }
}
