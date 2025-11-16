using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Arbitra.DataStructure
{
    internal class ListOfMatches
    {
        public List<Match> Matches { get; set; }

        public ListOfMatches()
        {
            Matches = new List<Match>();
        }

        public void AddMatch(Match match)
        {
            Matches.Add(match);
        }
        
        public void AddListOfMatches(ListOfMatches listOfMatches)
        {
            Matches.AddRange(listOfMatches.Matches);
        }

        public void RemoveListOfMatches(ListOfMatches listOfMatches)
        {
            Matches = Matches.Except(listOfMatches.Matches).ToList();
        }

        public void Merge(ListOfMatches mergingListOfMatches)
        {   
            foreach (Match match in Matches)
            {
                ListOfMatches removeMatches = new ListOfMatches();
                
                foreach (Match mergingMatch in mergingListOfMatches.Matches)
                {
                    if (match.IsSame(mergingMatch))
                    {
                        match.Merge(mergingMatch);
                        removeMatches.Matches.Add(mergingMatch);
                    }
                }

                mergingListOfMatches.RemoveListOfMatches(removeMatches);
            }
            Matches.AddRange(mergingListOfMatches.Matches);
        }

        

        public List<Event> SplitToEvents()
        {
            List<Event> listOfEvents = new List<Event>();
            foreach (Match match in Matches)
            {
                List<Event> addingListOfEvents = match.SplitToEvents();
                listOfEvents.AddRange(addingListOfEvents);
            }
            return listOfEvents;
        }
    }
}
