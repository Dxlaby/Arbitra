using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arbitra.DataStructure
{
    public class Event
    {
        public string Name { get; set; }
        public string RecognitionTeamName1 { get; set; }
        public string RecognitionTeamName2 { get; set; }
        public float BestImpliedProbability { get; set; }
        public float BestProfitPercentage { get; set; }
        public string Date { get; set; }
        public List<Odds> SetOfOdds { get; set; }
        //Set of odds are odds, where exactly one outcome will happen
        //public string UrlReference { get; set; }

        public Event(string name, string recognitionTeamName1, string recognitionTeamName2, 
            float bestImpliedProbability, float bestProfitPercentage, string date, List<Odds> setOfOdds)
        {
            Name = name;
            RecognitionTeamName1 = recognitionTeamName1;
            RecognitionTeamName2 = recognitionTeamName2;
            BestImpliedProbability = bestImpliedProbability;
            BestProfitPercentage = bestProfitPercentage;
            Date = date;
            SetOfOdds = setOfOdds;      
        }

        // public float GetImpliedProbability()
        // {
        //     float bestImpliedProbability = 0;
        //     foreach (Odd odd in OddsTable)
        //     {
        //         bestImpliedProbability += 1 / odd.BettingOdd;
        //     }
        //
        //     return bestImpliedProbability;
        // }
        //
        // public float GetProfitPercentage()
        // {
        //     float bestProfitPercentage = (1 / GetImpliedProbability() - 1) * 100;
        //     return bestProfitPercentage;
        // }
    }
}
