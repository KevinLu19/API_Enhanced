using Api_Enhanced.Services;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using Xunit;
using Xunit.Abstractions;

namespace Test_Api_Enhanced;

public class TestMALActorScrape : IDisposable
{
	private WebDriver _driver;
	private string _website = "https://myanimelist.net/people.php";
	private readonly ITestOutputHelper _test_output;
	private Dictionary<string, string> _anime_character_map = new Dictionary<string, string>();

	public TestMALActorScrape(ITestOutputHelper output)
    {
		_driver = new ChromeDriver();
		_driver.Navigate().GoToUrl(_website);

		_test_output = output;
    }

	//   [Fact]
	//public void VerifyWebsite()
	//{
	//	// Initialize
	//	Assert.Equal("https://myanimelist.net/people.php", _driver.Url);
	//}


	// https://myanimelist.net/people.php?cat=person&q=<lastname%firstname>
	// So far, found that lastname of "takahashi" gives multiple results.
	[Theory]
	//[InlineData("Takahashi Rie")]
	[InlineData("Hanazawa Kana")]
	[InlineData("Toyosaki Aki")]
	public void SearchActorViaSearchBar(string name)
	{
		var search_bar = _driver.FindElement(By.XPath("//input[@id='vaq']"));
		search_bar.Clear();
		search_bar.SendKeys(name);

		var search_enter_btn = _driver.FindElement(By.XPath("//button[@class='inputButton']"));
		search_enter_btn.Click();

		// Need to figure out what to do with actor/actresses with multiple results.

		Assert.NotNull(search_bar);
	}

	[Fact]
	public void SearchViaLink()
	{
		var firstname = "hanazawa";
		var lastname = "kana";

		var url = $"https://myanimelist.net/people.php?cat=person&q={lastname}%{firstname}";

		_driver.Navigate().GoToUrl(url);

		SortByMostFav();

		Assert.NotNull(_driver.Url);
	}

	public void SortByMostFav()
	{
		var sorted_by_most_recent = _driver.FindElement(By.XPath("//span[@id='js-people-character-sort-title']"));
		sorted_by_most_recent.Click();

		try
		{
			var favorites = _driver.FindElement(By.XPath("//span[@id='people-va-popularity']"));
			favorites.Click();

			_test_output.WriteLine("----Option selected for most favorites----");

			ResultTable();
			Thread.Sleep(2000);			// Sleep for 5 seconds.
		}
		catch (Exception ex) 
		{
			_test_output.WriteLine(ex.Message);
		}
	}

	public void ResultTable()
	{
		var anime_title = "js-people-title";			// <div class = {anime_title}>
		var character_name = "spaceit_pad";
		var anime_tv_series = "spaceit_pad anime-info-text";		// <div class = {anime_tv_series}>

		// Need to store the results in a hash table. The key would be anime titles, value would be character name.
		//ReadOnlyCollection<IWebElement> results = _driver.FindElements(By.XPath("//table[@class='js-table-people-character table-people-character']"));

		var title = _driver.FindElements(By.XPath($"//a[@class='{anime_title}']"));
		// var char_name = _driver.FindElements(By.XPath($"//div[@class='{character_name}']"));
		var tv_series = _driver.FindElements(By.XPath($"//div[@class='{anime_tv_series}']"));

		// Add anime title and series (tv) onto hashmap. Use this hashmap in order to filter out anime series franchise.
		// Zip title and tv_series into hashmap.
		var dict = title.Zip(tv_series, (k, v) => new { k, v })
			.ToDictionary(x => x.k, x => x.v);

		// Key: Anime title
		// Value: Tv series, Speical, etc
		foreach (var item in dict)
		{
			_test_output.WriteLine(item.Key.Text);
			_test_output.WriteLine("-------------");
			_test_output.WriteLine(item.Value.Text);
			_test_output.WriteLine("+++++++++++++");
		}

		//ReturnOnlyOneCharacterPerAnime(results);
	}

	// Want to also print out to user only 1 character per slot. Don't want to have 10 differnt series
	// of anime of the same character.
	public void ReturnOnlyOneCharacterPerAnime(ReadOnlyCollection<IWebElement> data_results)
	{
		var character_name = "";
		var anime_title = "";
		

		foreach (var item in data_results)
		{
			_test_output.WriteLine(item.Text);
		}

	}

	public void Dispose()
	{
		_driver.Quit();
	}
}