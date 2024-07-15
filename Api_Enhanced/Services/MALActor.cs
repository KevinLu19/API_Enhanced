using Api_Enhanced.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;


namespace Api_Enhanced.Services;

public class MALActor
{
    // People's website on MAL.
    private readonly string _website = "https://myanimelist.net/people.php";
    private IWebDriver? _driver;
    private string _driver_path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "drivers");

    public async Task<Actor> FetchPeopleInfo(string name)
    {
        //var options = new ChromeOptions();
        //options.AddArgument("--headless");      // Headless mode.

		// Navigate to chrome.exe
		_driver = new ChromeDriver(_driver_path);
		_driver.Navigate().GoToUrl(_website);

		// Locate search box and input name.
		IWebElement search_box = _driver.FindElement(By.XPath("//input[@name='q']"));
		search_box.SendKeys(name);
		search_box.Submit();

        // Wait for result to load.
        var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
        wait.Until(d => d.FindElement(By.XPath("//table[contains(@class, 'people-list')]/tbody/tr/td/div/a")));

		var people_list = _driver.FindElements(By.XPath("//table[contains(@class, 'people-list')]/tbody/tr/td/div/a"));

        Actor actor = new Actor();

        foreach (var person in people_list)
        {
            actor.Name = person.Text;
            actor.Url = person.GetAttribute("href");
            Console.WriteLine($"Name: {actor.Name} - url: {actor.Url}");
        }

        return actor;
    }
}
