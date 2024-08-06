using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test_Api_Enhanced;

// Contains all of the necessary items in order to test in one place.

interface IWebScrape
{
	void TraverseUrl(string incomplete_url);
}