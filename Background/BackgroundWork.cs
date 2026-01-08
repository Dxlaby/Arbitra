using System.Text.Json;
using Arbitra.Background.MatchFinders;
using Arbitra.DataStructure;

namespace Arbitra.Background
{
    public class BackgroundWork : BackgroundService
    {
        readonly ILogger<BackgroundWork> _logger;

        public BackgroundWork(ILogger<BackgroundWork> logger)
        {
            _logger = logger;
        }

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                string timeInfo = "od ";
                timeInfo += DateTime.Now.ToString("d. M. H:mm");
                
                OddsFinder oddsFinder = new OddsFinder();
                oddsFinder.FindOdds();
                
                timeInfo += "\ndo ";
                timeInfo += DateTime.Now.ToString("d. M. H:mm");
                File.WriteAllText(@"wwwroot/Data/LastLoaded.txt", timeInfo);
                
                await Task.Delay(TimeSpan.FromHours(8));
            }
        }
    }
}
