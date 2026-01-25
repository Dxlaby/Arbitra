using OpenQA.Selenium.Firefox;
using OpenQA.Selenium;
using Arbitra.DataStructure;
using OpenQA.Selenium.Edge;
using System.Text.Json;
using OpenQA.Selenium.Chrome;

namespace Arbitra.Background.MatchFinders
{
    public class OddsFinder
    {
        public void FindOdds()
        {
            List<IMatchFinder> matchFinders = new List<IMatchFinder>();
            
            // matchFinders.Add(new BetanoMatchFinder());
            // matchFinders.Add(new TipsportMatchFinder());
            // matchFinders.Add(new FortunaMatchFinder());
            matchFinders.Add(new SynottipMatchFinder());

            Console.WriteLine(Directory.GetCurrentDirectory());

            // string geckoDriverDirectory = @"Drivers/geckodriver-v0.36.0-linux-aarch64";
            string geckoDriverDirectory = @"./Drivers";

            var firefoxOptions = new FirefoxOptions();
            // firefoxOptions.AddArgument("--headless");
            firefoxOptions.SetPreference("devtools.console.stdout.content", true);
            // firefoxOptions.SetLoggingPreference(LogType.Browser, LogLevel.All);
            
            var options = new ChromeOptions();
            // This is the most important line to bypass detection
            options.AddArgument("--disable-blink-features=AutomationControlled");

            // Remove the "controlled by automated software" notification
            options.AddExcludedArgument("enable-automation");
            options.AddAdditionalOption("useAutomationExtension", false);

            TimeSpan commandTimeOut = TimeSpan.FromSeconds(600);
            
            ListOfMatches finalListOfMatches = new ListOfMatches();
            
            foreach (var matchFinder in matchFinders)
            {
                // var listOfMatches = matchFinder.FindAllMatchesSelenium(geckoDriverDirectory, options , commandTimeOut);
                var listOfMatches = matchFinder.FindAllMatchesApi();
                finalListOfMatches.Merge(listOfMatches);
            }

            List<Event> finalListOfEvents = finalListOfMatches.SplitToEvents();
            finalListOfEvents.Sort((a, b) => a.BestImpliedProbability.CompareTo(b.BestImpliedProbability));
            var listOfEvents = finalListOfEvents.Take(500);
            string json = JsonSerializer.Serialize<IEnumerable<Event>>(listOfEvents);
            // File.WriteAllText(@"wwwroot/Data/BettingOdds.json", json);
            //https://stackoverflow.com/questions/16921652/how-to-write-a-json-file-in-c
        }
        
        public List<Event>? GetEvents(int page, int sizeOfPage)
        {
            string json = File.ReadAllText(@"wwwroot/Data/BettingOdds.json");
            List<Event>? bettingOdds = JsonSerializer.Deserialize<List<Event>>(json);
            if (bettingOdds == null)
                return new List<Event>();
            if (sizeOfPage * page <= bettingOdds.Count() - sizeOfPage) 
            {
                List<Event> events = bettingOdds.GetRange(sizeOfPage * (page), sizeOfPage);
                return events;
            }
            else if (sizeOfPage*page < bettingOdds.Count()) //returns rest of matches, when there are remaining less that pageSize
            {
                List<Event> events = bettingOdds.GetRange(sizeOfPage * (page), bettingOdds.Count - sizeOfPage * (page));
                return events;
            }
            else //return null when page is too big
            {
                return null;
            }
        }

        public Event? GetEventByIndex(int index)
        {
            string json = File.ReadAllText(@"wwwroot/Data/BettingOdds.json");
            List<Event>? bettingOdds = JsonSerializer.Deserialize<List<Event>>(json);
            if (bettingOdds == null)
                return null;
            if (bettingOdds.Count < index || index < 0)
                return null;
            return bettingOdds[index];
        }
    }
}