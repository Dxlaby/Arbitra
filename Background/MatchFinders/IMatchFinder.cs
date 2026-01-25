using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arbitra.DataStructure;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;

namespace Arbitra.Background.MatchFinders
{
    interface IMatchFinder
    {
        public ListOfMatches FindAllMatchesSelenium(string geckoDriverDirectory, ChromeOptions options,
            TimeSpan commandTimeOut)
        {
            return new ListOfMatches();
        }
        public ListOfMatches FindAllMatchesApi()
            {
                return new ListOfMatches();
            }
        
    }
}
