using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using Xunit.Abstractions;
using OpenQA.Selenium.Firefox;

namespace Test_Api_Enhanced;

public class TestSelenium : IDisposable
{
    private IWebDriver _driver;
	private readonly ITestOutputHelper _test_output;
    private string _website = "https://www.instagram.com/katerina.soria/";

	public TestSelenium(ITestOutputHelper output)
    {
        _test_output = output;

        _driver = new FirefoxDriver();
        
    }

    [Fact]
    public void Test()
    {
		_driver.Navigate().GoToUrl(_website);
	}

    public void Dispose()
    {
        Thread.Sleep(5000);

        _driver.Quit();
    }
}
