using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;
using Arbitra.DataStructure;
using OpenQA.Selenium.Firefox;


namespace Arbitra.MatchFinders
{
    internal class TipsportMatchFinder : IMatchFinder
    {
        string bettingShopName;
        string urlAll;
        string urlTomorrow;
        By matchElementPath;
        By namesElementPath;
        By oddsElementPath;
        By referenceLinkElementPath;
        By timeElementPath;
        string oddClassName;
        string blockedOddClassName;

        public TipsportMatchFinder()
        {
            bettingShopName = "Tipsport";
            urlAll = "https://www.tipsport.cz/kurzy?timeFilter=form.period.anytime&limit=10000";
            urlTomorrow = "https://www.tipsport.cz/kurzy/zitra?limit=10000";
            matchElementPath = By.CssSelector(".o-matchRow");
            namesElementPath = By.CssSelector(".o-matchRow__matchName");
            oddsElementPath = By.CssSelector(".m-matchRowOdds");
            referenceLinkElementPath = By.CssSelector("a");
            timeElementPath = By.CssSelector(".o-matchRow__dateClosed");
            oddClassName = "btnRate";
            blockedOddClassName = "btnRate disabled";
        }

        public ListOfMatches FindAllMatches(string geckoDriverDirectory, FirefoxOptions options, TimeSpan commandTimeOut)
        {
            ListOfMatches listOfMatches = new ListOfMatches();
            using (var driver = new FirefoxDriver(geckoDriverDirectory, options, commandTimeOut))
            {
                listOfMatches.AddListOfMatches(FindMatchesByUrl(driver, urlAll));
            }

            return listOfMatches;
        }

        // public ListOfMatches FindTomorrowMatches(IWebDriver driver)
        // {
        //     ListOfMatches listOfMatches = FindMatchesByUrl(driver, urlTomorrow);
        //     return listOfMatches;
        // }

        private ListOfMatches FindMatchesByUrl(IWebDriver driver, string url)
        {
            driver.Navigate().GoToUrl(url);
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
            wait.Until(ExpectedConditions.ElementIsVisible(matchElementPath));
            IReadOnlyCollection<IWebElement>? matchesElements = driver.FindElements(matchElementPath);
            ListOfMatches listOfMatches = new ListOfMatches();

            foreach (IWebElement matchElement in matchesElements)
            {
                Match? match = GetMatchFromElement(matchElement);
                if (match != null)
                    listOfMatches.Matches.Add(match);
            }

            return listOfMatches;
        }

        private Match? GetMatchFromElement(IWebElement matchElement)
        {
            IWebElement matchNameElement = matchElement.FindElement(namesElementPath);
            string matchName = matchNameElement.Text;
            IWebElement referenceElement = matchElement.FindElement(referenceLinkElementPath);
            string referenceUrl = referenceElement.GetAttribute("href");
            
            var dateAndTimeElement = matchElement.FindElement(timeElementPath);
            var timeElements = dateAndTimeElement.FindElements(By.TagName("span"));
            DateTime dateTime = GetDate(timeElements.ElementAt(0).GetAttribute("innerHTML"),
                timeElements.ElementAt(1).GetAttribute("innerHTML"));
            if ((dateTime - DateTime.Now).TotalHours < 2)
                return null;

            var oddsElement = matchElement.FindElement(oddsElementPath);
            var roughOdds = GetOddsFromElement(oddsElement, referenceUrl);
            MatchOdds sortedOdds = SortOdds(roughOdds);

            var recognitionTeams = GetRecognitionTeams(matchName);

            if (sortedOdds.OddsTable.Length == 2)
                return new Match(matchName, recognitionTeams.Item1, 
                    recognitionTeams.Item2, dateTime, sortedOdds);
            else if (sortedOdds.OddsTable.Length == 6)
                return new Match(matchName, recognitionTeams.Item1, 
                    recognitionTeams.Item2, dateTime, sortedOdds);
            else
                return null;
        }

        private Odds?[] GetOddsFromElement(IWebElement oddsElement, string referenceUrl)
        {
            var maybeOddElements = oddsElement.FindElements(By.TagName("div"));
            List<Odds?> oddsList = new List<Odds?>();

            foreach (IWebElement maybeOddElement in maybeOddElements)
            {
                if (maybeOddElement.GetAttribute("class") == oddClassName)
                {
                    List<Odd> oddList = new List<Odd>();
                    oddList.Add(GetOddFromElement(maybeOddElement, referenceUrl));
                    oddsList.Add(new Odds(oddList));
                }
                else if (maybeOddElement.GetAttribute("class") == blockedOddClassName)
                    oddsList.Add(null);
            }

            return oddsList.ToArray();
        }

        private Odd? GetOddFromElement(IWebElement element, string referenceUrl)
        {
            float? odd = float.Parse(element.Text, CultureInfo.InvariantCulture.NumberFormat);
            return new Odd(bettingShopName, referenceUrl, odd.Value);
        }

        private MatchOdds SortOdds(Odds?[] roughOdds)
        {
            if (roughOdds.Count() == 2)
            {
                return new MatchOdds(roughOdds);
            }
            else if (roughOdds.Count() == 3)
            {
                Odds?[] finalOdds =
                { roughOdds[0], roughOdds[1], roughOdds[2], 
                    null, null, null};
                return new MatchOdds(finalOdds);
            }
            else if (roughOdds.Count() == 5)
            {
                Odds?[] finalOdds =
                { roughOdds[0], roughOdds[2], roughOdds[4], 
                    roughOdds[1], roughOdds[3], null};
                //prohazeni kurzu tak aby byly ve formatu 1, 0, 2, 10, 20, 12
                return new MatchOdds(finalOdds);
            }
            else
            {
                Odds?[] finalOdds = { null, null};
                return new MatchOdds(finalOdds);
            }
        }

        private Tuple<string, string> GetRecognitionTeams(string matchName)
        {
            matchName = RemoveDiacritics(matchName);
            matchName = matchName.ToLower();
            string[] teamNames = matchName.Split(" - ", 2);
            if (teamNames.Length == 2)
            {
                Tuple<string, string> recognitionTeamsTuple = new Tuple<string, string>(teamNames[0], teamNames[1]);
                return recognitionTeamsTuple;
            }
            else
            {
                return new Tuple<string, string>(teamNames[0], "");
            }
        }
        
        private DateTime GetDate(string date, string time)
        {
            string[] dates = date.Split(". ", 3);
            string[] times = time.Split(":", 2);
            return new DateTime(int.Parse(dates[2]), int.Parse(dates[1]), int.Parse(dates[0]),
                int.Parse(times[0]),int.Parse(times[1]), 0);
        }

        private string RemoveDiacritics(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder(capacity: normalizedString.Length);

            for (int i = 0; i < normalizedString.Length; i++)
            {
                char c = normalizedString[i];
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder
                .ToString()
                .Normalize(NormalizationForm.FormC);

            //https://stackoverflow.com/questions/249087/how-do-i-remove-diacritics-accents-from-a-string-in-net
        }
    }
}
