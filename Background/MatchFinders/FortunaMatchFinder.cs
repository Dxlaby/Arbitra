using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arbitra.DataStructure;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using OpenQA.Selenium.Firefox;

namespace Arbitra.Background.MatchFinders
{
    internal class FortunaMatchFinder : IMatchFinder
    {
        string _bettingShopName; 
        string url;
        string mainUrl;
        By cookieButtonElementPath;
        By matchElementPath;
        By namesElementPath;
        By showMoreElementPath;
        By oddsElementPath;
        By oddsContainerPath;
        By referenceLinkElementPath;
        By dateElementPath;
        public FortunaMatchFinder()
        {
            _bettingShopName = "Fortuna";
            url = "https://www.ifortuna.cz/sazeni?filter=all";//"https://www.ifortuna.cz/sazeni/fotbal";//
            mainUrl = "https://www.ifortuna.cz";
            cookieButtonElementPath = By.CssSelector(".deny");
            matchElementPath = By.CssSelector(".no-underline.fixture-safe-link.cursor-pointer.fixture-card");
            namesElementPath = By.CssSelector(".m-0.text-sm");
            showMoreElementPath = By.CssSelector(".offer-card-group-header__caret.text-content-secondary.cursor-pointer.transition-all");
            oddsElementPath = By.CssSelector(".odds-button2__value.block.text-xs.uppercase.text-ellipsis.max-w-full.overflow-hidden.text-content-primary");
            oddsContainerPath = By.CssSelector(".fixture-card__market-outcomes");
            referenceLinkElementPath = By.CssSelector(".no-underline.fixture-safe-link.cursor-pointer.fixture-card");
            dateElementPath = By.CssSelector(".fixture-card__time");
        }

        public ListOfMatches FindAllMatchesSelenium(string geckoDriverDirectory, ChromeOptions options, TimeSpan commandTimeOut)
        {
            // initialize driver and stu
            using (var driver = new ChromeDriver(geckoDriverDirectory, options, commandTimeOut))
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
                driver.Navigate().GoToUrl(url);
            
            
                //click on cookie button
                try
                {
                    // wait.Until(ExpectedConditions.ElementIsVisible(cookieButtonElementPath));
                    IWebElement buttonConsent = driver.FindElement(cookieButtonElementPath);
                    buttonConsent.Click();
                }
                catch
                {
                
                }
            
                ScrollDown(driver, wait);
                
            
                //finally find all matches odds
                return FindListOfMatches(driver);
            }
        }
            
        public void ScrollDown(IWebDriver driver, WebDriverWait wait)
        {
            IJavaScriptExecutor jse = (IJavaScriptExecutor)driver;
        
            while (true)
            {
                var previousMatchesNames = driver.FindElements(By.CssSelector(".event-name"));
                //var bottomElement = driver.FindElement(By.CssSelector(".button.button-yellow"));
                // var BottomElement = driver.FindElement(By.ClassName("message-box-message"));
                //jse.ExecuteScript("arguments[0].scrollIntoView(true)", bottomElement);
                jse.ExecuteScript("window.scrollBy(0, document.body.scrollHeight)");
                for (int i=0; i < 7; i++)
                {
                    jse.ExecuteScript("window.scrollBy(0,-300)");
                    Thread.Sleep(TimeSpan.FromMilliseconds(200));
                }
                //jse.ExecuteScript(
                    //"require('sport-infinite-scroll')($('#sport-events-list-content'),'\\/bets\\/ajax\\/loadmoreofferedsports\\/?timeTo=&rateFrom=&rateTo=&date=&pageSize=100',51,$('#sport-events-list-ajax-loading'),$('#sport-events-list-ajax-error'),'#sport-events-list-ajax-load-more');");
                try
                {
                    wait.Until(d => d.FindElements(By.CssSelector(".event-name")).Count > previousMatchesNames.Count);
                }
                catch
                {
                       
                    break;
                }
            }
        }
        
        public ListOfMatches FindListOfMatches(IWebDriver driver)
        {
            var matchesElements = driver.FindElements(matchElementPath);
            ListOfMatches listOfMatches = new ListOfMatches();
            
            foreach (IWebElement matchElement in matchesElements)
            {
                try
                {
                    Match? match = GetMatchFromElement(matchElement);
                    if (match != null)
                        listOfMatches.AddMatch(match);
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                }
            }
            return listOfMatches;
        }
        
        private Match? GetMatchFromElement(IWebElement matchElement)
        {
          //not all matchElements have matches. Some of them are just rows with information
            try
            {
                matchElement.FindElement(namesElementPath);
            }
            catch
            {
                try
                {
                    IWebElement showMoreElement = matchElement.FindElement(showMoreElementPath);
                    showMoreElement.Click();
                    matchElement.FindElement(namesElementPath);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            IWebElement matchNameElement = matchElement.FindElement(namesElementPath);
            string matchName = matchNameElement.Text;
            // IWebElement referenceElement = matchElement.FindElement(referenceLinkElementPath);
            string referenceUrl = matchElement.GetAttribute("href");
            DateTime dateTime = DateTime.Now;
            try
            {
                IWebElement dateElement = matchElement.FindElement(dateElementPath);
                dateTime = GetDate(dateElement.Text);
                if ((dateTime - DateTime.Now).TotalHours < 2)
                    return null;
            }
            catch
            {
                return null;
            }


            var roughOdds = GetOddsFromElement(matchElement, referenceUrl);
            MatchOdds? sortedOdds = SortOdds(roughOdds);
            
            var recognitionTeams = GetRecognitionTeams(matchName);
        
            if (sortedOdds == null)
                return null;
            else if (sortedOdds.OddsTable.Length == 2)
                return new Match(matchName, recognitionTeams.Item1, 
                    recognitionTeams.Item2, dateTime, sortedOdds);
            else if (sortedOdds.OddsTable.Length == 6)
                return new Match(matchName, recognitionTeams.Item1, 
                    recognitionTeams.Item2, dateTime, sortedOdds);
            else
                return null;
        }
        
        private Odds?[] GetOddsFromElement(IWebElement matchElement, string referenceUrl)
        {
            var containerElements = matchElement.FindElements(oddsContainerPath);
            List<Odds?> oddsList = new List<Odds?>();
            List<IWebElement>? allOddElements = new List<IWebElement>(); 
            foreach (IWebElement containerElement in containerElements)
            {
                var oddElements = matchElement.FindElements(oddsElementPath);
                allOddElements.AddRange(oddElements);
                
                if (allOddElements.Count == 2)
                    break;
                else if (allOddElements.Count == 6)
                    break;
            }

            foreach (IWebElement oddElement in allOddElements)
            {
                try
                {
                    float bettingOdd = float.Parse(oddElement.Text, CultureInfo.InvariantCulture.NumberFormat);
                    if (bettingOdd > 1)
                    {
                        List<Odd> oddList = new List<Odd>();
                        Odd odd = new Odd(_bettingShopName, referenceUrl, bettingOdd);
                        oddList.Add(odd);
                        oddsList.Add(new Odds(oddList));
                    }
                    else
                        oddsList.Add(null);
                }
                catch
                {
                    oddsList.Add(null);
                }
            }

            return oddsList.ToArray();
        }
        
        private MatchOdds? SortOdds(Odds?[] roughOdds)
        {
            if (roughOdds.Length == 2)
            {
                return new MatchOdds(roughOdds);
            }
            else if (roughOdds.Length == 6)
            {
                Odds placeHolderOdd = roughOdds[4];
                roughOdds[4] = roughOdds[5];
                roughOdds[5] = placeHolderOdd;
                return new MatchOdds(roughOdds);
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

        private DateTime GetDate(string dateText)
        {
            string[] dateAndTime = dateText.Split(" ");
            string time = dateAndTime[^1];
            string[] times = time.Split(":", 2);
            
            int minute = int.Parse(times[1]);
            int hour = int.Parse(times[0]);
            int year = DateTime.Now.Year;
            int month = DateTime.Now.Month;
            int day = DateTime.Now.Day;
            DateTime today = new DateTime(year, month, day, hour, minute, 0);
            
            if (dateAndTime[0] == "dnes")
                return today;
            if (dateAndTime[0] == "zítra")
                return today.AddDays(-1);
                
            var culture = new System.Globalization.CultureInfo("cs-CZ");
            return DateTime.Parse(dateText, culture);
            string date = dateAndTime[0];
            
            string[] dates = date.Split(".");
            
            if (int.Parse(dates[1]) == 2 && int.Parse(dates[0]) == 29 && DateTime.Now.Year%4 == 0) 
                return new DateTime(DateTime.Now.Year, int.Parse(dates[1]), int.Parse(dates[0]),
                    int.Parse(times[0]), int.Parse(times[1]), 0);
            else if (int.Parse(dates[1]) == 2 && int.Parse(dates[0]) == 29 && (DateTime.Now.Year+1)%4 == 0) 
                return new DateTime(DateTime.Now.Year+1, int.Parse(dates[1]), int.Parse(dates[0]),
                    int.Parse(times[0]), int.Parse(times[1]), 0);
            //This is for leap years. Man I hate time
        
            DateTime dateYearNow = new DateTime(DateTime.Now.Year, int.Parse(dates[1]), int.Parse(dates[0]),
                int.Parse(times[0]),int.Parse(times[1]), 0);
            DateTime dateYearLater = new DateTime(DateTime.Now.Year + 1, int.Parse(dates[1]), int.Parse(dates[0]),
                int.Parse(times[0]),int.Parse(times[1]), 0);
            if (dateYearNow > DateTime.Now)
                return dateYearNow;
            return dateYearLater;
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
