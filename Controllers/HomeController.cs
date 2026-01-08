using Arbitra.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Arbitra.Background;
using Arbitra.DataStructure;
using System.Text.Json;
using Arbitra.Background;
using System.IO;
using Arbitra.Background.MatchFinders;

namespace Arbitra.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult AboutUs()
        {
            return View();
        }

        public IActionResult Counter(int index)
        {
            OddsFinder oddsFinder = new OddsFinder();
            Event? myEvent = oddsFinder.GetEventByIndex(index);
            if (myEvent == null)
                return null;
            return View(myEvent);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        
        [HttpPost]
        public JsonResult GetJsonEvents(int Page, int PageSize) //int Page, int SizeOfPage
        { 
            OddsFinder oddsFinder = new OddsFinder();
            var events = oddsFinder.GetEvents(Page, PageSize);
            return new JsonResult(events);
        }

        [HttpPost]
        public string GetHtmlEvents(int Page, int PageSize )
        {
            
            OddsFinder oddsFinder = new OddsFinder();
            List<Event> events = oddsFinder.GetEvents(Page, PageSize);
            string html = "";
            foreach (Event matchEvent in events)
            {
                html += "<div class=\"event-preview\">" +
                        "<div class=\"information-row\">"+
                        "<\"div class=\"name\">";
                //     <h6>@matchEvent.Name</h6>
                //     </div> "              
                //
                //     if (matchEvent.GetProfitPercentage() > 0)
                // {
                //     <div class="profit green">
                //         <b>Profit: Math.Round(matchEvent.GetProfitPercentage(), 2).ToString() %</b>
                //         </div>
                // }
                // else
                // {
                //     <div class="profit red">
                //         <b>Profit: Math.Round(matchEvent.GetProfitPercentage(), 2).ToString() %</b>
                //         </div>                    
                // }
                // </div>
                //     <div class="odds">
                //     @foreach (Odd odd in matchEvent.OddsTable)
                // {
                //     <a href=@odd.UrlReference class="odd @odd.BettingShop">
                //         <div class="odd @odd.BettingShop">
                //         @odd.BettingShop: @odd.BettingOdd
                //         </div>                    
                //         </a>
                // }
                
            }

            return html;
        }
    }
}