using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium;
using SeleniumExtras.WaitHelpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using Arbitra.DataStructure;
using OpenQA.Selenium.Firefox;
using HtmlAgilityPack;
using OpenQA.Selenium.Chrome;

namespace Arbitra.Background.MatchFinders
{
    internal class BetanoMatchFinder : IMatchFinder
    {
        private readonly string _bettingShopName;
        private readonly List<string> _sportUrls;
        private readonly string _matchElementPath;
        private readonly string _namesElementPath;
        private readonly string _nameElementPath;
        private readonly string _oddsElementPath;
        private readonly string _oddElementPath;
        private readonly string _oddElementPath2;
        private readonly string _showButtonElementPath;
        //By sportBoxElementPath;
        private readonly string _regionBoxElementPath;
        private readonly string _regionNameElementPath;
        private readonly string _checkBoxElementPath;
        private readonly string _closePopUpButton;
        private readonly string _referenceLinkElementPath;
        private readonly string _timeElementPath;
        //By gridElementPath;

        //string oddClassName;
        //string blockedOddClassName;

        public BetanoMatchFinder()
        {
            _bettingShopName = "Betano";
            _sportUrls = new List<string> 
            { 
                "https://www.betano.cz/sport/fotbal/", 
                "https://www.betano.cz/sport/ledni-hokej/",
                "https://www.betano.cz/sport/tenis/", 
                "https://www.betano.cz/sport/mma/", 
                "https://www.betano.cz/sport/stolni-tenis/", 
                "https://www.betano.cz/sport/esports/", 
                "https://www.betano.cz/sport/baseball/", 
                "https://www.betano.cz/sport/sipky/", 
                "https://www.betano.cz/sport/americky-fotbal/", 
                "https://www.betano.cz/sport/volejbal/",
                "https://www.betano.cz/sport/hazena/", 
                "https://www.betano.cz/sport/futsal/", 
                "https://www.betano.cz/sport/plazovy-volejbal/",
                "https://www.betano.cz/sport/vodni-polo/", 
                "https://www.betano.cz/sport/box/", 
                "https://www.betano.cz/sport/rugby-union/"
            };
            _matchElementPath = ".//*[@class='" + "vue-recycle-scroller__item-view" + "']";
            _namesElementPath = ".//*[@class='" + "tw-flex tw-flex-1 tw-flex-col tw-min-w-0" + "']";
            _nameElementPath = ".//*[@class='" + "tw-text-n-13-steel tw-inline-block tw-align-top tw-w-auto tw-pl-xs" + "']";
            _oddsElementPath = ".//*[@class='" + "tw-flex-1" + "']";
            _oddElementPath = ".//*[@class='" + "selections__selection__odd" + "']";
            _oddElementPath2 = ".//*[@class='" + "table__markets__market events-list__grid__event--separator" + "']";
            _showButtonElementPath = ".//*[@class='" + "tw-bg-n-17-black-pearl" + "']";
            //sportBoxElementPath = By.CssSelector(".sport-block__item");
            _regionBoxElementPath = ".//*[@class='" + "tw-flex tw-flex-col tw-pb-m tw-pt-n tw-bg-white-snow" + "']";
            _regionNameElementPath = ".//*[@class='" + "tw-pl-[4px] tw-text-s tw-leading-s tw-text-n-13-steel tw-no-underline hover:tw-font-bold tw-line-clamp-1" + "']";
            _checkBoxElementPath = ".//*[@class='" + "tw-inline-flex" + "']";
            _closePopUpButton = ".//*[@class='" + "sb-modal__close__btn" + "']";
            //gridElementPath = By.CssSelector(".grid__column");

            //oddClassName = "btnRate";
            //blockedOddClassName = "btnRate disabled";
            _referenceLinkElementPath = ".//*[@class='" + "tw-flex-1 tw-p-n tw-no-underline tw-text-inherit" + "']";
            _timeElementPath = ".//*[@class='" + "tw-flex tw-flex-row tw-justify-start tw-items-center tw-text-xs tw-leading-s tw-flex-col-reverse tw-justify-center tw-text-n-48-slate"
                                              + "']";
        }

        public ListOfMatches FindAllMatches(string geckoDriverDirectory, ChromeOptions options, TimeSpan commandTimeOut) //
        {
            HtmlWeb web = new HtmlWeb();
            
            List<string> regionUrls = new List<string>();
            ListOfMatches finalListOfMatches = new ListOfMatches();
            foreach (string url in _sportUrls)
            {
                var sportHtml = new HtmlDocument();
                using (var driver = new ChromeDriver(geckoDriverDirectory, options))
                {
                    driver.Navigate().GoToUrl(url);
                    var sportContent = driver.PageSource;
                    sportHtml.LoadHtml(sportContent);
                    driver.Quit();
                }
                
                var regionNameElements = sportHtml.DocumentNode.SelectNodes(_regionNameElementPath);
                
                if (regionNameElements == null)
                    continue;
                
                foreach (var regionName in regionNameElements)
                {
                    if (regionName == null)
                        continue;
                    string regionUrl = "https://betano.cz" + regionName.Attributes["href"].Value;
                    regionUrls.Add(regionUrl); 
                }
                
            }
            
            foreach (string regionUrl in regionUrls)
            {
                var regionUrlHtml = new HtmlDocument();
               
                using (var driver = new ChromeDriver(geckoDriverDirectory, options))
                {
                    driver.Navigate().GoToUrl(regionUrl);
                    var regionUrlContent = driver.PageSource;
                    regionUrlHtml.LoadHtml(regionUrlContent);
                    driver.Quit();
                }
                
                ListOfMatches listOfMatches = FindMatches(regionUrlHtml);
                finalListOfMatches.AddListOfMatches(listOfMatches);
            }
            
            return finalListOfMatches;
        }
            
        private ListOfMatches FindMatches(HtmlDocument htmlDocument)
        {
            var matchesElements = htmlDocument.DocumentNode.SelectNodes(_matchElementPath);
            if (matchesElements == null)
                return new ListOfMatches();
            ListOfMatches listOfMatches = new ListOfMatches();
            
            foreach (var matchElement in matchesElements)
            {
                Match? match = GetMatchFromElement(matchElement);
                if (match != null)
                    listOfMatches.Matches.Add(match);
            }
        
            return listOfMatches;
        }
        
        private Match? GetMatchFromElement(HtmlNode matchElement)
        {
            var matchNameElements = matchElement.SelectNodes(_nameElementPath);
            if (matchNameElements == null || matchNameElements.Count < 2)
                return null;
            string matchName = matchNameElements.ElementAt(0).InnerText + " - " + matchNameElements.ElementAt(1).InnerText;
            var referenceElement = matchElement.SelectSingleNode(_referenceLinkElementPath);
            string referenceUrl = "https://betano.cz" + referenceElement.Attributes["href"].Value;
            
            var timeElement = matchElement.SelectSingleNode(_timeElementPath);
            var timeElements = timeElement.SelectNodes(".//span");
            DateTime? dateTime = GetDate(timeElements.ElementAt(0).InnerText, timeElements.ElementAt(1).InnerText);
            if (dateTime == null)
                return null;
            if ((dateTime.Value - DateTime.Now).TotalHours < 2)
                return null;
            
            Odds?[]? roughOdds = null;
            
            var oddsElement = matchElement.SelectSingleNode(_oddsElementPath);
            if (oddsElement == null)
                return null;
            roughOdds = GetOddsFromElement(oddsElement, referenceUrl);
            
            var sortedOdds = SortOdds(roughOdds);
        
            var recognitionTeams = GetRecognitionTeams(matchName);
        
            if (sortedOdds == null)
                return null;
            else if (sortedOdds.OddsTable.Length == 2)
                return new Match(matchName, recognitionTeams.Item1, 
                    recognitionTeams.Item2, dateTime.Value, sortedOdds);
            else if (sortedOdds.OddsTable.Length == 6)
                return new Match(matchName, recognitionTeams.Item1,
                    recognitionTeams.Item2, dateTime.Value, sortedOdds);
            else
                return null;
        }
        
        private Odds?[]? GetOddsFromElement(HtmlNode oddsElement, string referenceUrl)
        {
            var oddElements = new HtmlNodeCollection(oddsElement);
            try
            {
                oddElements = oddsElement.SelectNodes(_oddElementPath);
            }
            catch
            {
                try
                {
                    oddElements = oddsElement.SelectNodes(_oddElementPath2);
                }
                catch
                {
                    return null;
                }
            }
            
            List<Odds?> oddsList = new List<Odds?>();
            
            if (oddElements == null)
                return oddsList.ToArray();
            
            foreach (var oddElement in oddElements)
            {
                var odd = GetOddFromElement(oddElement, referenceUrl);
                var listOfOdd = new List<Odd>();
                listOfOdd.Add(odd);
                var odds = new Odds(listOfOdd);
                oddsList.Add(odds);
            }
        
            return oddsList.ToArray();
        }
        
        private Odd? GetOddFromElement(HtmlNode element, string referenceUrl)
        {
            float? odd = float.Parse(element.InnerText.Replace(" ", ""), CultureInfo.InvariantCulture.NumberFormat);
            if (odd == null)
                return null;
            return new Odd(_bettingShopName, referenceUrl, odd.Value);
        }
        
        private MatchOdds? SortOdds(Odds?[] roughOdds)
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
                return null;
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
        
        private DateTime? GetDate(string date, string time) //not ideal to how try method here
        {
            string[] dates = date.Split(".", 2);
            string[] times = time.Split(":", 2);
            try
            {
                if (int.Parse(dates[1]) == 2 && int.Parse(dates[0]) == 29 && DateTime.Now.Year%4 == 0) 
                    return new DateTime(DateTime.Now.Year, int.Parse(dates[1]), int.Parse(dates[0]),
                        int.Parse(times[0]), int.Parse(times[1]), 0);
                else if (int.Parse(dates[1]) == 2 && int.Parse(dates[0]) == 29 && (DateTime.Now.Year+1)%4 == 0) 
                    return new DateTime(DateTime.Now.Year+1, int.Parse(dates[1]), int.Parse(dates[0]),
                    int.Parse(times[0]), int.Parse(times[1]), 0);
                //This is for leap years. Man I hate time
                
                DateTime dateYearNow = new DateTime(DateTime.Now.Year, int.Parse(dates[1]), int.Parse(dates[0]),
                    int.Parse(times[0]), int.Parse(times[1]), 0);
                DateTime dateYearLater = new DateTime(DateTime.Now.Year + 1, int.Parse(dates[1]), int.Parse(dates[0]),
                    int.Parse(times[0]), int.Parse(times[1]), 0);
                if (dateYearNow > DateTime.Now)
                    return dateYearNow;
                return dateYearLater;
            }
            catch
            {
                return null;
            }
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
