using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Arbitra.DataStructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using OpenQA.Selenium.Firefox;
using ProtoBuf;
using Microsoft.ClearScript.V8;
using ProtoBuf.WellKnownTypes;

namespace Arbitra.Background.MatchFinders
{
    internal class KingsbetMatchFinder : IMatchFinder
    {
        string _bettingShopName;
        private string _sportsUrl;
        private string _eventUrl;
        private string _referenceUrl;

        public KingsbetMatchFinder()
        {
            _bettingShopName = "Kingsbet";
            _sportsUrl =
                "https://sb2frontend-altenar2.biahosted.com/api/widget/GetSportMenu?culture=cs-CZ&timezoneOffset=-60&integration=kingsbet&deviceType=1&numFormat=en-GB&countryCode=CZ&period=0";
            _eventUrl =
                "https://sb2frontend-altenar2.biahosted.com/api/widget/GetEvents?culture=cs-CZ&timezoneOffset=-60&integration=kingsbet&deviceType=1&numFormat=en-GB&countryCode=CZ&eventCount=0&champIds=";
            _referenceUrl = "https://www.kingsbet.cz/sport?page=event&eventId=";
        }

        public ListOfMatches FindAllMatchesApi()
        {
            ListOfMatches listOfMatches = new ListOfMatches();
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "MyCSharpApp/1.0");
            var sportsIds = GetSportsIds(httpClient);
            foreach (var sportId in sportsIds)
            {
                ListOfMatches newList = GetMatchesFromId(httpClient, sportId);
                listOfMatches.AddListOfMatches(newList);
            }

            return listOfMatches;
        }

        private ListOfMatches GetMatchesFromId(HttpClient httpClient, int id)
        {
            ListOfMatches listOfMatches = new ListOfMatches();
            var eventsJson = GetNodeFromUrl(httpClient, _eventUrl + id.ToString());
            if (eventsJson == null) return listOfMatches;
            
            var eventsArray = eventsJson["events"].AsArray();
            var competitorsArray = eventsJson["competitors"].AsArray();
            var oddsArray = eventsJson["odds"].AsArray();
            var marketsArray = eventsJson["markets"].AsArray();
            
            Dictionary<long, string> competitorsDic = new Dictionary<long, string>();
            Dictionary<long, float> oddsDic = new Dictionary<long, float>();
            Dictionary<long, (long[], string)> marketsDic = new Dictionary<long, (long[], string)>();
            
            foreach (var competitor in competitorsArray)
            {
                long compId = competitor["id"].GetValue<long>();
                string teamName = competitor["name"].GetValue<string>();
                competitorsDic.Add(compId, teamName);
            }

            foreach (var odds in oddsArray)
            {
                long oddsId = odds["id"].GetValue<long>();
                float price = odds["price"].GetValue<float>();
                oddsDic.Add(oddsId, price);
            }

            foreach (var market in marketsArray)
            {
                long marketId = market["id"].GetValue<long>();
                long[] oddIds = market["oddIds"].AsArray()
                    .Select(node => node.GetValue<long>()).ToArray();
                string type = market["name"].GetValue<string>();
                marketsDic.Add(marketId, (oddIds, type));
            }

            foreach (var sprtEvent in eventsArray)
            {
                var recognitionTeamIds =
                    sprtEvent["competitorIds"].AsArray()
                        .Select(node => node.GetValue<long>()).ToArray();
                string recognitionTeam1 = competitorsDic[recognitionTeamIds[0]];
                string recognitionTeam2 = competitorsDic[recognitionTeamIds[1]];
                string name = recognitionTeam1 + " - " + recognitionTeam2;
                string dateString = sprtEvent["startDate"].GetValue<string>();
                DateTime startDate = DateTime.Parse(dateString, null, System.Globalization.DateTimeStyles.AdjustToUniversal); // Always returns UTC time
                string eventId = sprtEvent["id"].GetValue<long>().ToString();
                string referenceLink = _referenceUrl + eventId;
                long[] marketIds = sprtEvent["marketIds"].AsArray()
                    .Select(node => node.GetValue<long>()).ToArray();
                Odds?[]? matchOdds = GetOddsFromDic(marketIds, marketsDic, oddsDic, referenceLink);
                if (matchOdds == null) continue;
                Match match = new Match(name, recognitionTeam1, recognitionTeam2, startDate, matchOdds);
                listOfMatches.AddMatch(match);
            }

            return listOfMatches;
        }

        private Odds?[]? GetOddsFromDic(long[] marketIds,Dictionary<long, (long[], string)> marketsDic, Dictionary<long, float> oddsDic,
            string refLink)
        {
            if (marketIds.Length == 0) return null;
            var marketId1 = marketIds[0];
            var market1 = marketsDic[marketId1];
            if (market1.Item2 != "Výsledek zápasu") return null;
            if (market1.Item1.Length == 2)
            {
                float[] odds = market1.Item1
                    .Select(id => oddsDic[id]).ToArray();
                return GetOddsFromIntArr(odds, refLink);
            }
            else if (market1.Item1.Length == 3)
            {
                float[] odds1 = market1.Item1
                    .Select(id => oddsDic[id]).ToArray();
                if (marketIds.Length < 2) 
                    return GetOddsFromIntArr(odds1, refLink);
                var market2 = marketsDic[marketIds[1]];
                if (market2.Item2 != "Výsledek zápasu – dvojtip") 
                    return GetOddsFromIntArr(odds1, refLink);
                float[] odds2 = market2.Item1
                    .Select(id => oddsDic[id]).ToArray();
                float[] odds = odds1.Concat(odds2).ToArray();
                return GetOddsFromIntArr(odds, refLink);
            }
            return null;
        }

        private Odds?[]? GetOddsFromIntArr(float[] odds, string refLink)
        {
            if (odds.Length == 2)
            {
                Odds?[] oddsArr = new Odds[2];
                for (int i=0; i<odds.Length; i++ )
                {
                    oddsArr[i] = Odds.FromSingle(_bettingShopName, refLink, odds[i]);
                }
                return oddsArr;
            }
            else if (odds.Length == 3)
            {
                Odds?[] oddsArr = new Odds[6];
                for (int i=0; i<odds.Length; i++)
                {
                    oddsArr[i] = Odds.FromSingle(_bettingShopName, refLink, odds[i]);
                }
                return oddsArr;
            }
            else if (odds.Length == 6)
            {
                Odds?[] oddsArr = new Odds[6];
                for (int i = 0; i < odds.Length; i++)
                {
                    // flip the 5th and 6th number so that 12 and 02 prices are at the right place
                    if (i == 4) oddsArr[i] = Odds.FromSingle(_bettingShopName, refLink, odds[5]);
                    else if (i == 5) oddsArr[i] = Odds.FromSingle(_bettingShopName, refLink, odds[4]);
                    else oddsArr[i] = Odds.FromSingle(_bettingShopName, refLink, odds[i]);
                }

                return oddsArr;
            }
            else return null;
        }
        

        private List<int> GetSportsIds(HttpClient httpClient)
        {
            var sportIds = new List<int>();
            var sportsJson = GetNodeFromUrl(httpClient, _sportsUrl);
            if (sportsJson == null) return sportIds;
            var champsArray = sportsJson["champs"].AsArray();
            
            foreach (var champ in champsArray)
            {
                int id = champ["id"].GetValue<int>();            
                sportIds.Add(id);
            }

            return sportIds;
        }
        
        private JsonNode? GetNodeFromUrl(HttpClient httpClient, string jsonUrl)
        {
            var response = httpClient.GetAsync(jsonUrl).Result;
            try
            {
                response.EnsureSuccessStatusCode();
            }
            catch (Exception e)
            {
                Console.WriteLine("Could not load url: " + jsonUrl);
                Console.WriteLine(e.ToString());
                return null;
            }
            string jsonString = response.Content.ReadAsStringAsync().Result;
            var finalJson = JsonNode.Parse(jsonString);

            return finalJson;
        }
    }
}