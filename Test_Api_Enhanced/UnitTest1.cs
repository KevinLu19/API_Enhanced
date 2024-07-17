﻿using Api_Enhanced.Services;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using Xunit.Abstractions;
using System.Linq;
using System.Text.RegularExpressions;

namespace Test_Api_Enhanced;

public class TestMALActorScrape : IDisposable
{
	private WebDriver _driver;
	private string _website = "https://myanimelist.net/people.php";
	private readonly ITestOutputHelper _test_output;
	private Dictionary<IWebElement, IWebElement> _anime_character_map = new Dictionary<IWebElement, IWebElement>();

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


	//// https://myanimelist.net/people.php?cat=person&q=<lastname%firstname>
	//// So far, found that lastname of "takahashi" gives multiple results.
	//[Theory]
	////[InlineData("Takahashi Rie")]
	//[InlineData("Hanazawa Kana")]
	//[InlineData("Toyosaki Aki")]
	//public void SearchActorViaSearchBar(string name)
	//{
	//	var search_bar = _driver.FindElement(By.XPath("//input[@id='vaq']"));
	//	search_bar.Clear();
	//	search_bar.SendKeys(name);

	//	var search_enter_btn = _driver.FindElement(By.XPath("//button[@class='inputButton']"));
	//	search_enter_btn.Click();

	//	// Need to figure out what to do with actor/actresses with multiple results.

	//	Assert.NotNull(search_bar);
	//}

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
			Thread.Sleep(2000);			// Sleep for 2 seconds.
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
		_anime_character_map = title.Zip(tv_series, (k, v) => new { k, v })
			.ToDictionary(x => x.k, x => x.v);

		// Key: Anime title
		// Value: Tv series, Speical, etc
		//foreach (var item in _anime_character_map)
		//{
		//	_test_output.WriteLine(item.Key.Text);
		//	_test_output.WriteLine("-------------");
		//	_test_output.WriteLine(item.Value.Text);
		//	_test_output.WriteLine("+++++++++++++");
		//}

		SingleAnimeFranchise();

		//ReturnOnlyOneCharacterPerAnime(results);
	}

	// Removes the special, ova, etc from the anime franchise located in the hashmap.
	// Uses TV as the main source for filtration.
	public void SingleAnimeFranchise()
	{
		// Stores main anime series.
		List<string> main_anime_series = new List<string>();

		// Sort by TV series.
		foreach (var item in _anime_character_map)
		{
			// Value using key.
			var anime_tv_franchise = _anime_character_map[item.Key];

			//_test_output.WriteLine(anime_tv_franchise.Text);

			// Checks if value got TV in its string.
			if (anime_tv_franchise.Text.Contains("TV"))
			{
				// Add the key (main anime title name).
				main_anime_series.Add(item.Key.Text);
			}
			
		}

		//// Printing the list.
		//foreach (var items in main_anime_series)
		//{
		//	_test_output.WriteLine(items);
		//}


		List<string> keywords_filtration = new List<string> { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0", "second", "third", "season", ":", "recap", "!!" };
		var main_list = FilterMainAnime(main_anime_series, keywords_filtration);

		// Sort list by anime. 
		// Remove any numbers, letters, etc in the list.
		// Only show one anime franchise in the series.
		for (int i = 0; i < main_list.Count; i++) 
		{
			_test_output.WriteLine(main_list[i]);			
		}
	}

	// Grabs only the main series for the anime and return it back to the SingleAnimeFranchise().
	public List<string> FilterMainAnime(List<string> anime_list, List<string> keywords)
	{
		var anime_hash_set = new HashSet<string>();
		var main_list = new List<string>();

		
		foreach (var item in anime_list)
		{
			if (!ContainsKeyword(item, keywords))
			{
				if (!anime_hash_set.Contains(item))
				{
					anime_hash_set.Add(item);
					main_list.Add(item);
				}
			}

		}

		return main_list;
	}

	// Check if list contains any of the keywords
	public bool ContainsKeyword(string anime, List<string> keywords)
	{
		foreach (var keyword in keywords)
		{
			if (anime.ToLower().Contains(keyword.ToLower()))
			{
				return true;
			}
		}

		return false;
	}

	public void Dispose()
	{
		_driver.Quit();
	}
}