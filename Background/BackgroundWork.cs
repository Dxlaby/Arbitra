using System.Text.Json;
using Arbitra.Background.MatchFinders;
using Arbitra.DataStructure;

namespace Arbitra.Background
{
    public class BackgroundWork : BackgroundService
    {
        readonly ILogger<BackgroundWork> _logger;
        private readonly ScraperStatus _status;

        public BackgroundWork(ILogger<BackgroundWork> logger, ScraperStatus status)
        {
            _logger = logger;
            _status = status;
        }

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            
            while (!stoppingToken.IsCancellationRequested)
            {
                _status.IsRunning = true; 
                _status.LastRunStarted = DateTime.Now;
                OddsFinder oddsFinder = new OddsFinder();
                try 
                {
                    await Task.Run(() => new OddsFinder().FindOdds(), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break; 
                }               
                _status.IsRunning = false;
                
                await Task.Delay(TimeSpan.FromHours(8), stoppingToken); // stopping token so that it can be stopped
            }
        }
    }
    
    public class ScraperStatus
    {
        public bool IsRunning { get; set; }
        public DateTime? LastRunStarted { get; set; }
    }
}
