using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using Xunit.Abstractions;
using OpenQA.Selenium.Firefox;
using MySql.Data.MySqlClient;
using System.Xml.Linq;
using OpenQA.Selenium.Support.UI;

namespace Test_Api_Enhanced;

public interface IDatabase
{
	MySqlConnection DatabaseConnection();
	void Insert(MySqlConnection conn, string last_name, string first_name, string popularity);
}

public interface IActorNames
{
	string SupplyActorNames();
}

public class TestMALActorScrape : IDisposable, IDatabase, IActorNames
{
	private IWebDriver _driver;
	// private string _website = "https://myanimelist.net/people.php";
	private readonly ITestOutputHelper _test_output;
	private Dictionary<IWebElement, IWebElement> _anime_character_map = new Dictionary<IWebElement, IWebElement>();
	// private string _driver_path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "drivers");

	public TestMALActorScrape(ITestOutputHelper output)
    {
		_test_output = output;

		var fire_fox_service = FirefoxDriverService.CreateDefaultService();

		_driver = new FirefoxDriver(fire_fox_service);

		var firefox_options = new FirefoxOptions();
		firefox_options.AddArgument("headless");
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
		//var firstname = "hanazawa";
		//var lastname = "kana";
		var firstname = "atsumi";
		var lastname = "tanezaki";

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

	/*
		Tasks: 
		- Anime names that contains a number. Ex: 5-toubun no Hanayome. Need to exclude that from filtration of anime. -> Use title character count. If number's position is towards lower floor half, disregard.
	 */

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
				// Use title character count. If number's position is towards lower floor half, disregard.
				// Pass in found keyword to function.

				return true;
			}
		}

		return false;
	}

	public bool LocateNumPosition(string anime_name, string target_keywords)
	{
		// Middle of the string
		char[] title_char_array = anime_name.ToCharArray();

		var left_char_array = title_char_array.Take((title_char_array.Length + 1) / 2).ToArray();
		var right_char_array = title_char_array.Skip(title_char_array.Length / 2).ToArray();

		char key = Convert.ToChar(target_keywords);

		/*
		- Divide the anime title by 2 to get the middle. 
		- if target_keyword is in left side of the split string, dont filter out. 
		 */

		bool char_contains = left_char_array.Contains(key);

		// Indicates keyword is part of the anime title.
		if (char_contains)
		{
			return true;
		}

		// Indicates keyword will most likely be part of a series anime.
		return false;
	}

	// From IActorName interface
	public string SupplyActorNames()
	{
		List<string> actor_lastname = new List<string> { "tanezaki", "han", "iwami", "fujita", "hanazawa" };
		List<string> actor_firstname = new List<string> { "atsumi",  "megumi", "manaka", "saki", "kana" };

		// Choose a random name from list.
		Random r = new Random();
		int r_int = r.Next(0, actor_lastname.Count);

		List<string> lastname_firstname = new List<string>();

		// Zip 2 lists together
		lastname_firstname = actor_lastname.Zip(actor_firstname, (x, y) => x + " " + y).ToList();

		return lastname_firstname[r_int];
	}

	[Fact]
	// for api/people/popularity
	public void GetActorPopularity()
	{
		//var actress = "https://myanimelist.net/people/34785/Rie_Takahashi?q=takahashi&cat=person";

		//string last_name = "tanezaki";
		//string first_name = "atsumi";

		// Get random actor name supplied by the interface.
		var actor_names = SupplyActorNames();

		var name_split = actor_names.Split(" ");
		var last_name = name_split[0];
		var first_name = name_split[1];

		string complete_url = $"https://myanimelist.net/people.php?cat=person&q={last_name}%{first_name}";

		_driver.Navigate().GoToUrl(complete_url);

		// Adding a wait to ensure the page loads completely
		WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));

		// Locate Member Function: <number>
		// //div[span[text()='Member Favorites:']] find the exact html and grabs the entire div node.
		var fav = _driver.FindElement(By.XPath("//div[span[text()='Member Favorites:']]"));

		// Gets Member Function: <number>
		//_test_output.WriteLine(fav.Text);

		// Only get the value
		var value = fav.Text.Replace("Member Favorites:", "").Trim();

		//_test_output.WriteLine(value);

		 

		// Add popularity to voiceactor table on the database.
		var conn = DatabaseConnection();

		using (conn = DatabaseConnection())
		{
			if (conn.State != System.Data.ConnectionState.Open)
				conn.Open();

			try
			{
				Insert(conn, last_name, first_name, value);
			}
			catch (Exception ex)
			{
				_test_output.WriteLine(ex.Message);
			}
			finally
			{
				conn.Close();
			}
		}

	}

	// From IDatabase Interface.
	public MySqlConnection DatabaseConnection()
	{
		MySqlConnection connection;
		string? username = Environment.GetEnvironmentVariable("DATABASE_USERNAME");
		string? password = Environment.GetEnvironmentVariable("DATABASE_PASSWORD");

		string conn_string = $"server=localhost;user={username};database=api_enhanced;port=3306;password={password}";

		connection = new MySqlConnection(conn_string);

		try
		{
			connection.Open();

			// Create table if it doesn't already exists.
			string create_table_query = @"CREATE TABLE IF NOT EXISTS test_voiceactors 
			(
				ActorID INT AUTO_INCREMENT PRIMARY KEY,
				last_name VARCHAR(50),
				first_name VARCHAR(50),
				popularity VARCHAR(100),
				UNIQUE (last_name, first_name)
			)";

			var create_table_cmd = new MySqlCommand(create_table_query, connection);

			create_table_cmd.ExecuteNonQuery();
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.Message);
		}

		return connection;
	}

	// From IDatabase Insert() Interface.
	public void Insert(MySqlConnection conn ,string last_name, string first_name, string popularity)
	{
		var insert_string_query = @"INSERT INTO test_voiceactors (last_name, first_name, popularity) VALUES (@lastname, @firstname, @popularity) 
		ON DUPLICATE KEY UPDATE last_name = VALUES(last_name), first_name = VALUES(first_name);";

		MySqlCommand cmd = new MySqlCommand(insert_string_query, conn);

		// Add Parameters.
		cmd.Parameters.AddWithValue("@lastname", last_name);
		cmd.Parameters.AddWithValue("@firstname", first_name);
		cmd.Parameters.AddWithValue("@popularity", popularity);

		cmd.ExecuteNonQuery();

		try
		{
			if (conn.State != System.Data.ConnectionState.Open)
				conn.Open();

			var change_in_rows = cmd.ExecuteNonQuery();

			if (change_in_rows > 0)
				Console.WriteLine("Inserted data or updated popularity on a row.");
			else
				Console.WriteLine("No row(s) were affected");
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.Message);
		}
		finally
		{
			conn.Close();
		}
	}

	public void Dispose()
	{
		Thread.Sleep(5000);

		_driver.Quit();
	}
}