using System.Xml.Linq;

namespace Arbitra.DataStructure
{
    public class MatchOdds
    {
        public Odds?[] OddsTable { get; set; }
        //Odds table are odds, where more than one outcome can happen
        //It's called table because it has lots of different combinations of 1, 0, 2, even though it's just an array
        //BettingOdds are in oder 1, 0, 2, 10, 02, 12 for three outcome matches
        //for two outcome matches is just 1, 2
        public MatchOdds(Odds?[] oddsTable) 
        {
            OddsTable = oddsTable;

            if (OddsTable.Length != 2 && OddsTable.Length != 6)
                throw new Exception("You can only give array as argument, if it has 2 or 6 oddsTable");
        }

        public void Merge(MatchOdds matchOdds) 
        {
            for (int i = 0; i < OddsTable.Length; i++)
            {
                if (matchOdds.OddsTable[i] == null)
                {
                    continue;
                }
                else if (OddsTable[i] == null)
                {
                    OddsTable[i] = matchOdds.OddsTable[i];
                }
                else
                {
                    OddsTable[i].BettingOdds.AddRange(matchOdds.OddsTable[i].BettingOdds);
                    OddsTable[i].Sort();
                }
            }
        }

        public List<Event> SplitToEvents(string name, string teamName1, string teamName2, DateTime date)
        {
            List<Event> events = new List<Event>();

            if (OddsTable.Length == 2)
            {
                if (OddsTable[0] != null && OddsTable[1] != null)
                {
                    List<Odds> odds = new List<Odds>();
                    odds.Add(OddsTable[0]);
                    odds.Add(OddsTable[1]);

                    float bestImpliedProbability = GetBestImpliedProbability(odds);
                    float bestProfitPercentage = GetBestProfitPercentage(odds);
                    
                    Event newEvent = new Event(name, teamName1,  teamName2,
                        bestImpliedProbability, bestProfitPercentage, date.ToString("d. M. H:mm"), odds);
                    events.Add(newEvent);
                }
            }
            else if (OddsTable.Length == 6)
            {
                //BettingOdds are in oder 1, 0, 2, 10, 02, 12 for three outcome matches
                if (OddsTable[0] != null && OddsTable[1] != null && OddsTable[2] != null)
                {
                    List<Odds> odds = new List<Odds>();
                    odds.Add(OddsTable[0]);
                    odds.Add(OddsTable[1]);
                    odds.Add(OddsTable[2]);

                    float impliedProbability = GetBestImpliedProbability(odds);
                    float profitPercentage = GetBestProfitPercentage(odds);
                    
                    Event newEvent = new Event(name, teamName1,  teamName2,
                        impliedProbability, profitPercentage, date.ToString("d. M. H:mm"), odds);                    
                    events.Add(newEvent);
                }
                if (OddsTable[0] != null && OddsTable[4] != null)
                {
                    List<Odds> odds = new List<Odds>();
                    odds.Add(OddsTable[0]);
                    odds.Add(OddsTable[4]);

                    float impliedProbability = GetBestImpliedProbability(odds);
                    float profitPercentage = GetBestProfitPercentage(odds);
                    
                    Event newEvent = new Event(name, teamName1,  teamName2,
                        impliedProbability, profitPercentage, date.ToString("d. M. H:mm"), odds);                    
                    events.Add(newEvent);
                }
                if (OddsTable[1] != null && OddsTable[5] != null)
                {
                    List<Odds> odds = new List<Odds>();
                    odds.Add(OddsTable[1]);
                    odds.Add(OddsTable[5]);

                    float impliedProbability = GetBestImpliedProbability(odds);
                    float profitPercentage = GetBestProfitPercentage(odds);
                    
                    Event newEvent = new Event(name, teamName1,  teamName2,
                        impliedProbability, profitPercentage, date.ToString("d. M. H:mm"), odds);
                    events.Add(newEvent);
                }
                if (OddsTable[2] != null && OddsTable[3] != null)
                {
                    List<Odds> odds = new List<Odds>();
                    odds.Add(OddsTable[2]);
                    odds.Add(OddsTable[3]);

                    float impliedProbability = GetBestImpliedProbability(odds);
                    float profitPercentage = GetBestProfitPercentage(odds);
                    
                    Event newEvent = new Event(name, teamName1,  teamName2,
                        impliedProbability, profitPercentage, date.ToString("d. M. H:mm"), odds);
                    events.Add(newEvent);
                }
            }
            
            return events;
        }

        private float GetBestImpliedProbability(List<Odds> odds)
        {
            float impliedProbability = 0;
            foreach (Odds odd in odds)
            {
                impliedProbability += 1 / odd.BettingOdds[0].BettingOdd; //the [0] is to find maximum implied probability this event has
            }
            
            return impliedProbability;
        }
        
        public float GetBestProfitPercentage(List<Odds> odds)
        {
            float profitPercentage = (1 / GetBestImpliedProbability(odds) - 1) * 100;
            return (float)Math.Round(profitPercentage, 2);
        }
        
    }
}
