using Api_Enhanced.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;


namespace Api_Enhanced.Services;

/*
 Web scrape for actor/actress from MyanimeList website.
 */

// Interface for MALActor
public interface IMALActor
{
	Task<List<string>> FetchPeopleInfo(string name);
}

public class MALActor : IMALActor
{
	// People's website on MAL.
	private string _website = "https://myanimelist.net/people.php";
	private IWebDriver? _driver;
	private string _driver_path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "drivers");
	private Dictionary<IWebElement, IWebElement> _anime_character_map = new Dictionary<IWebElement, IWebElement>();
	private List<string> _main_anime_list = new List<string>();

	// Saving actor name from GetPeopleRoles() function from Actor Controller in order to populate the popularity function.
	private string _save_actor_name;

	public MALActor()
	{
		var firefox_default_service = FirefoxDriverService.CreateDefaultService();

		_driver = new FirefoxDriver(firefox_default_service);

		var firefox_options = new FirefoxOptions();
		firefox_options.AddArgument("headless");
	}

	// Endpoint: api/people/<name>
	public async Task<List<string>> FetchPeopleInfo(string name)
	{
		// Entered name should be Lastname Firstname. Split on the space in between.
		var name_split = name.Split(" ");
		var last_name = name_split[0];
		var first_name = name_split[1];

		// Search by Link.
		var complete_website = $"{_website}?cat=person&q={last_name}%{first_name}";

		_driver?.Navigate().GoToUrl(complete_website);

		// SortByMostFavorite();
		GetAllMainAnimes();         // End result will get all of the main anime.

		_save_actor_name = name;	// Saving name for api/people/popularity function. 

		return _main_anime_list;
	}

	public void GetAllMainAnimes()
	{
		SortByMostFavorite();
	}

	// Sort anime by favorites from actor / actress page.
	public void SortByMostFavorite()
	{
		var sorted_by_most_recent = _driver.FindElement(By.XPath("//span[@id='js-people-character-sort-title']"));
		sorted_by_most_recent.Click();

		// Sort the listed animes from actor / actress by most favorite by members.
		try
		{
			var favorites = _driver.FindElement(By.XPath("//span[@id='people-va-popularity']"));
			favorites.Click();

			ResultTable();				// Result Data
			Thread.Sleep(2000);         // Sleep for 2 seconds.
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.Message);
		}
	}

	// Result from actor / actress page after Selenium.
	public void ResultTable()
	{
		var anime_title = "js-people-title";						// <div class = {anime_title}>
		var anime_tv_series = "spaceit_pad anime-info-text";        // <div class = {anime_tv_series}>

		// Filter out the non TV series animes such omitting OVA, Movies, Specials etc. Still have TV special however.

		var title = _driver.FindElements(By.XPath($"//a[@class='{anime_title}']"));
		var tv_series = _driver.FindElements(By.XPath($"//div[@class='{anime_tv_series}']"));

		// Add anime title and series (tv) onto hashmap. Use this hashmap in order to filter out anime series franchise.
		// Zip title and tv_series into hashmap.
		_anime_character_map = title.Zip(tv_series, (k, v) => new { k, v })
			.ToDictionary(x => x.k, x => x.v);


		SingleAnimeFranchise();
	}

	// Removes special, ova, etc. Only grabs TV as the main source.
	public void SingleAnimeFranchise()
	{
		// Stores main anime series.
		List<string> main_anime_series = new List<string>();

		// Sort by TV series.
		foreach (var item in _anime_character_map)
		{
			// Value using key.
			var anime_tv_franchise = _anime_character_map[item.Key];

			// Checks if value got TV in its string.
			if (anime_tv_franchise.Text.Contains("TV"))
			{
				// Add the key (main anime title name).
				main_anime_series.Add(item.Key.Text);
			}

		}

		// Filter words to remove from actor / actress page.
		List<string> keywords_filtration = new List<string> { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0", "second", "third", "season", ":", "recap", "!!" };
		var main_list = FilterMainAnime(main_anime_series, keywords_filtration);

		// Sort list by anime. 
		// Remove any numbers, letters, etc in the list.
		// Only show one anime franchise in the series.
		for (int i = 0; i < main_list.Count; i++)
		{
			Console.WriteLine(main_list[i]);
		}
	}

	// Grabs only the main series for the anime and return it back to SingleAnimeFranchise()
	// Ex: One Piece <insert movie> compared to One piece (main series).
	public List<string> FilterMainAnime(List<string> anime_list, List<string> keywords)
	{
		var anime_hash_set = new HashSet<string>();
		//var main_list = new List<string>();


		foreach (var item in anime_list)
		{
			if (!ContainsKeyword(item, keywords))
			{
				if (!anime_hash_set.Contains(item))
				{
					anime_hash_set.Add(item);
					_main_anime_list.Add(item);				// Add to List<string> from private access modifier.
				}
			}

		}

		return _main_anime_list;
	}

	// Check if list contains any of the keywords
	public bool ContainsKeyword(string anime, List<string> keywords)
	{
		foreach (var keyword in keywords)
		{
			if (anime.ToLower().Contains(keyword.ToLower()))
			{
				// Use title character count. If number's position is towards lower floor half, disregard.
				// Pass in found keyword to function.

				return true;
			}
		}

		return false;
	}


	// api/people/popularitiy
	
}