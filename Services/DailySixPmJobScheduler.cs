using System.Text.Json;
using Capsitech.Data.MongoDB;
using Capsitech.Services;

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
            _logger.LogInformation(
               "DailySixPmJobScheduler executing. Version: {Version}",
               AppConfig.Current.Version
           );
            _logger.LogInformation("Attempting to send cron test email.");
            if (AppConfig.Current.Version == "admin-live")
            {
                var res=await _emailSender.SendEmailAsync(
                    "jeeeeet6902@gmail.com",
                    "Congratulations!!",
                    "This is to inform you that you have successfully ran a cron job."
                );
                _logger.LogInformation(
                "Email response:\n{Response}",
                JsonSerializer.Serialize(
                    res,
                    new JsonSerializerOptions
                    {
                        WriteIndented = true
                    }
                )
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