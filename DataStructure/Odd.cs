namespace Arbitra.DataStructure
{
    public class Odd
    {
        //public string TeamName1 { get; set; }
        //public string TeamName2 { get; set; }
        public string BettingShop { get; set; }
        public string UrlReference { get; set; }
        public float BettingOdd { get; set; }
        
        public Odd(string bettingShop, string urlReference, float bettingOdd)
        {
            BettingShop = bettingShop;
            UrlReference = urlReference;
            BettingOdd = bettingOdd;
        }
    }
}
