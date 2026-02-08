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
            
            // matchFinders.Add(nNormalizeStringew BetanoMatchFinder());
            // matchFinders.Add(new TipsportMatchFinder());
            matchFinders.Add(new FortunaMatchFinder());
            // matchFinders.Add(new SynottipMatchFinder());
            matchFinders.Add(new KingsbetMatchFinder());
            
            ListOfMatches finalListOfMatches = new ListOfMatches();
            
            foreach (var matchFinder in matchFinders)
            {
                // var listOfMatches = matchFinder.FindAllMatchesSelenium(geckoDriverDirectory, options , commandTimeOut);
                var matches = matchFinder.FindAllMatchesApi();
                finalListOfMatches.Merge(matches);
            }
            
            //Saving and retrieving final list of matches so as to debug split to events method without web crawling
            string jsonListOfMatches = JsonSerializer.Serialize<ListOfMatches>(finalListOfMatches);
            File.WriteAllText(@"wwwroot/Data/Matches.json", jsonListOfMatches);

            // string readJsonListOfMatches = File.ReadAllText(@"wwwroot/Data/Matches.json");
            // ListOfMatches? listOfMatches = JsonSerializer.Deserialize<ListOfMatches>(readJsonListOfMatches);
            // if (listOfMatches == null) return;
            
            
            List<Event> finalListOfEvents = finalListOfMatches.SplitToEvents();
            finalListOfEvents.Sort((a, b) => a.BestImpliedProbability.CompareTo(b.BestImpliedProbability));
            // var listOfEvents = finalListOfEvents.Take(500);
            string json = JsonSerializer.Serialize<IEnumerable<Event>>(finalListOfEvents);
            File.WriteAllText(@"wwwroot/Data/BettingOdds.json", json);
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