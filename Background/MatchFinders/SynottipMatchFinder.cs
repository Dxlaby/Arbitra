using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Arbitra.DataStructure;
using Newtonsoft.Json;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using OpenQA.Selenium.Firefox;

namespace Arbitra.Background.MatchFinders
{
    internal class SynottipMatchFinder : IMatchFinder
    {
        string _bettingShopName; 
        string url;
        string mainUrl;
        string urlAllMatchesAPI;

        public SynottipMatchFinder()
        {
            _bettingShopName = "Synottip";
            url = "https://sport.synottip.cz";
            urlAllMatchesAPI =
                "https://sport.synottip.cz/WebServices/Api/SportsBettingService.svc/GetWebStandardEvents";
        }

        public ListOfMatches FindAllMatchesApi()
        {
            using var httpClient = new HttpClient();
            
            // 1. Define the JSON body based on what you found in
            var requestBody = new
            {
                LanguageID = 12,
                Token = "4be2ffeaf2d471cac7316cb9ee9abb72", // Note: Tokens usually expire!
                CategoryID = "20",
                Top = 50,
                IncludeLiveCategories = true
            };
            var requestJson = JsonConvert.SerializeObject(requestBody);
            var payLoad = new StringContent(requestJson, Encoding.UTF8, "application/json");
            var response = httpClient.PostAsync(urlAllMatchesAPI, payLoad);
            var result = response.Result.Content.ReadAsStringAsync();
            return new ListOfMatches();
        }
    }
}
