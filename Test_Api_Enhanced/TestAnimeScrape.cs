using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using System;
using System.Collections.Generic;
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

    public void Dispose()
    {
        Thread.Sleep(2000);
        _driver.Quit();
    }
}
