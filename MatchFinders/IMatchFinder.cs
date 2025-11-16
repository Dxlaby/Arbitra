using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arbitra.DataStructure;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;

namespace Arbitra.MatchFinders
{
    interface IMatchFinder
    {
        public ListOfMatches FindAllMatches(string geckoDriverDirectory, FirefoxOptions options, TimeSpan commandTimeOut);
    }
}
