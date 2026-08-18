using Capsitech.Data.MongoDB;
using TaskManager.Models;

namespace TaskManager.Services
{
    public class DailySixPmJobScheduler : CronJobService
    {
        private readonly ILogger<DailySixPmJobScheduler> _logger;
        private readonly IEmailSender _emailSender;
        private readonly DBConfiguration _dbConfig;

        public DailySixPmJobScheduler(IScheduleConfig<DailySixPmJobScheduler> config, ILogger<DailySixPmJobScheduler> logger, IEmailSender emailSender, DBConfiguration dbConfig) : base(config.CronExpression, config.TimeZoneInfo)
        {
            _logger = logger;
            _dbConfig = dbConfig;
            _emailSender = emailSender;
            if (config.GetType().GenericTypeArguments[0].Name != GetType().Name)
            {
                throw new ArgumentException("Incorrect JobType name for IScheduleConfig.");
            }
            if (logger.GetType().GenericTypeArguments[0].Name != GetType().Name)
            {
                throw new ArgumentException("Incorrect JobType name for ILogger.");
            }
        }

        public override System.Threading.Tasks.Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("CronJob starts.");
            return base.StartAsync(cancellationToken);
        }

        public override async Task<System.Threading.Tasks.Task> DoWork(CancellationToken cancellationToken)
        {
            if (AppConfig.Current.Version == "admin-live")
            {

                await _emailSender.SendEmailAsync(
                    "biswarup.naha@capsitech.com",
                    "Congratulations!!",
                    "This is to inform you that you have successfully ran a cron job."
                );
            }

            return System.Threading.Tasks.Task.CompletedTask;
        }

        public override System.Threading.Tasks.Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("CronJob is stopping.");
            return base.StopAsync(cancellationToken);
        }
    }
}