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
using Microsoft.AspNetCore.Components.Forms;

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

			// _test_output.WriteLine("Clicked on all 3 options.");
		}
		catch (Exception ex)
		{
			_test_output.WriteLine(ex.Message);
		}

		//List<string> result = GetNewsPerDay();

		//return result;

		GetNewsPerDay();
	}

	public List<string> GetNewsPerDay()
	{
		Dictionary<string, string> news_feed = new Dictionary<string, string>();

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

		MakeListReadable(cleaned_list);

		return cleaned_list;
	}

	public void MakeListReadable(List<string> cleaned_list)
	{
		List<Dictionary<string, string>> results = new List<Dictionary<string, string>>();

		foreach (var item in cleaned_list)
		{
			// Split string by month and date.
			var sections = Regex.Split(item, @"(?=Sep \d{1,2},)");

			foreach (var section in sections)
			{
				var lines = section.Trim().Split("\n", StringSplitOptions.RemoveEmptyEntries);

				// Print lines for debugging purposes
				_test_output.WriteLine($"--- New Section ---\n{section}");
				for (int i = 0; i < lines.Length; i++)
				{
					_test_output.WriteLine($"{i}: {lines[i]}");
				}


				if (lines.Length >= 2)
				{
					// Line[0] gives the date
					string date = lines[0].Trim();
					string description = lines[1].Trim();

					// Title is at the end of the last non empty line.
					string title = lines[^1].Trim();

					// Store into dictionary
					results.Add(new Dictionary<string, string>
					{
						{ "Date", date},
						{ "Title", title},
						{ "Description", description}
					});
				}

				//_test_output.WriteLine(lines[1]);
			}
		}

		//// Print items in List<dictionary<string,string>>
		//foreach (var item in results)
		//{
		//	_test_output.WriteLine($"Date: {item["Date"]}");
		//	_test_output.WriteLine($"Title: {item["Title"]}");
		//	_test_output.WriteLine($"Description: {item["Description"]}");
		//	_test_output.WriteLine(new string('-', 50));
		//}

	}

	// For endpoint: api/anime/studio
	[Theory]
	[InlineData("Kyoto Animation")]
	public void GetStudio(string studio_name)
	{
		try
		{
			EnterStudioFromUser(studio_name);

		}
		catch (Exception e)
		{
			Console.WriteLine(e.Message);
		}
	}

	public void EnterStudioFromUser(string studio_name)
	{
		string studio_url = "https://myanimelist.net/company";
		_driver.Navigate().GoToUrl(studio_url);

		// Wait 2 seconds for page to load.
		Thread.Sleep(2000);


		IWebElement search_box = _driver.FindElement(By.XPath("//form[@class='di-ib']"));
		search_box.Click();
		search_box.SendKeys(studio_name);

		// Navigate to entered studio from user.
		IWebElement search_button = _driver.FindElement(By.XPath("//button[@class='inputButton']"));
		search_button.Click();

		// Wait 2 seconds for page to load.
		Thread.Sleep(2000);

		// Click on the entry
		IWebElement entry = _driver.FindElement(By.XPath("//td[@class='borderClass']"));
		entry.Click();
	}

	public void Dispose()
    {
        Thread.Sleep(2000);
        _driver.Quit();
    }
}
