using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Remote;
using SeleniumExtras;
using Arbitra.MatchFinders;

namespace Arbitra
{
    class Program
    {
        static void Main(string[] args)
        {
            // Console.Write("Hello world!");
            //vytvorit nazvy tymu v zapasech k rozpoznani
            //upravit split events v ThreeOutcomeMatchOdds
            //*
            OddsFinder oddsFinder = new OddsFinder();
            oddsFinder.FindOdds();

            
            
            // ListOfMatches finalListOfMatches = new ListOfMatches();
            // TipsportMatchFinder tipsportMatchFinder = new TipsportMatchFinder();
            // BetanoMatchFinder betanoMatchFinder = new BetanoMatchFinder();
            //
            // WebDriver driver = new EdgeDriver();
            // ListOfMatches tipsportMatches = tipsportMatchFinder.FindAllMatches(driver);
            // ListOfMatches betanoMatches = betanoMatchFinder.FindAllMatches(driver);
            // driver.Close();
            // ListOfMatches fortunaMatches = matchFinder.FortunaFindMatches();
            //
            // finalListOfMatches.Merge(tipsportMatches);
            // finalListOfMatches.Merge(fortunaMatches);
            // finalListOfMatches.Merge(betanoMatches);
            //
            // ListOfEvents finalListOfEvents = finalListOfMatches.SplitToEvents();
            // finalListOfEvents.SortByImpliedProbability();
            // finalListOfEvents.PrintToConsole();
            /*/
            WebDriver driver = new EdgeDriver();
            BetanoMatchFinder betanoMatchFinder = new BetanoMatchFinder();
            ListOfMatches betanoMatches = betanoMatchFinder.FindAllMatches(driver);
            driver.Close();
            betanoMatches.SortByName();
            betanoMatches.PrintToConsole();
            ListOfEvents finalListOfEvents = betanoMatches.SplitToEvents();
            finalListOfEvents.SortByImpliedProbability();
            finalListOfEvents.PrintToConsole();
            //*/
        }
    }
}