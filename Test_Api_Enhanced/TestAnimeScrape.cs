using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

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

    public void Dispose()
    {
        Thread.Sleep(2000);
        _driver.Quit();
    }
}
