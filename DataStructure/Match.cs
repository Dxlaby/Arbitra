namespace Arbitra.DataStructure
{
    public class Match
    {
        public string Name { get; set; }
        public string RecognitionTeamName1 { get; set; }
        public string RecognitionTeamName2 { get; set; }
        public DateTime Date { get; set; }
        public MatchOdds Odds { get; set; }
        public Match (string name, string recognitionTeamName1, string recognitionTeamName2, DateTime date, Odds?[] odds)
        {
            Name = name;
            RecognitionTeamName1 = NormalizeString(recognitionTeamName1);
            RecognitionTeamName2 = NormalizeString(recognitionTeamName2);
            Date = date;
            Odds = new MatchOdds(odds);
        }
        public Match(string name, string recognitionTeamName1, string recognitionTeamName2, DateTime date, MatchOdds matchOdds)
        {
            Name = name;
            RecognitionTeamName1 = NormalizeString(recognitionTeamName1);
            RecognitionTeamName2 = NormalizeString(recognitionTeamName2);
            Date = date;
            Odds = matchOdds;
        }

        public void Merge(Match match)
        {
            Odds.Merge(match.Odds);
        }

        public List<Event> SplitToEvents()
        {
            List<Event> events = Odds.SplitToEvents(Name, RecognitionTeamName1, RecognitionTeamName2, Date);
            return new List<Event>(events);
        }

        public bool IsSame(Match otherMatch)
        {
            if ( (RecognitionTeamName1.Contains(otherMatch.RecognitionTeamName1) 
                || otherMatch.RecognitionTeamName1.Contains(RecognitionTeamName1))
                && (RecognitionTeamName2.Contains(otherMatch.RecognitionTeamName2) 
                || otherMatch.RecognitionTeamName2.Contains(RecognitionTeamName2))
                && (Odds.OddsTable.Length == otherMatch.Odds.OddsTable.Length)
                && (Date.Date == otherMatch.Date.Date))
            {
                return true;
            }

            return false;
        }

        private string NormalizeString(string matchName)
        {
            //here is the core in finding if two events from different betting shops are the same events
            
            matchName = matchName.Replace("/", " ");
            matchName = matchName.Replace(".", " ");
            
            string[] teamWordsArray = matchName.Split(" ");
            List<string> finalTeamWordList = new List<string>();
            if (matchName.Length > 3)
            {
                List<string> teamWordList = teamWordsArray.ToList();

                foreach (string word in teamWordList)
                {
                    if (word.Length > 2 && !word.StartsWith("("))
                    {
                        finalTeamWordList.Add(word);
                    }
                }

            }
            else
            {
                finalTeamWordList = teamWordsArray.ToList();
            }

            string newMatchName = String.Join("", finalTeamWordList);
            if (newMatchName == "")
                return String.Join("", teamWordsArray);
                    
            return newMatchName;
        }
    }
}
