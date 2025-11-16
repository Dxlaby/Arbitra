

namespace Arbitra.DataStructure
{
    
    public class Odds
    {
        public List<Odd> BettingOdds { get; set; }

        public Odds(List<Odd> bettingOdds)
        {
            BettingOdds = bettingOdds;
        }
        
        public void Sort()
        {
            BettingOdds.Sort(delegate(Odd odd, Odd odd1)
            {
                return odd.BettingOdd.CompareTo(odd1.BettingOdd);
            });
        }
    }
}
