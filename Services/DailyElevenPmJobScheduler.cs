using Capsitech.Data.MongoDB;
using TaskManager.Models;

namespace TaskManager.Services
{
    public class DailyElevenPmJobScheduler : CronJobService
    {
        //private readonly ILogger<DailyTwoPmJobScheduler> _logger;
        private readonly IEmailSender _emailSender;
        private readonly DBConfiguration _dbConfig;

        public DailyElevenPmJobScheduler(IScheduleConfig<DailyElevenPmJobScheduler> config, ILogger<DailyElevenPmJobScheduler> logger, IEmailSender emailSender, DBConfiguration dbConfig) : base(config.CronExpression, config.TimeZoneInfo)
        {
            //_logger = logger;
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
            //_logger.LogInformation("CronJob 3 starts.");
            return base.StartAsync(cancellationToken);
        }

        public override async Task<System.Threading.Tasks.Task> DoWork(CancellationToken cancellationToken)
        {
            // call daily attendance notification email method
            if (AppConfig.Current.Version == "admin-live")
            {
                //await new EnDB(_dbConfig).DailyAttNotification();
                //await _emailSender.SendEmailAsync("kuldeep.gehlot@capsitech.com", "test", "cron ran");

                //var res = await new EnquiryCommunicationDB(_dbConfig).UpdateNotCalledCall();
                //string subject = "Cron Job Executed - Daily 11PM Scheduler";
                //string message;

                if (true)
                {
                    //message = $"Cron executed successfully at {DateTime.Now:dd-MM-yyyy HH:mm:ss} IST.\n\n";
                    Console.WriteLine("Cron successful");
                }
                else
                {
                    //message = $"Cron execution failed at {DateTime.Now:dd-MM-yyyy HH:mm:ss} IST.\n\nError: {res.Message}";
                    Console.WriteLine("Something went wrong");
                }

                // Send email
                //await _emailSender.SendEmailAsync("kuldeep.gehlot@capsitech.com", subject, message);

            }

            return System.Threading.Tasks.Task.CompletedTask;
        }

        public override System.Threading.Tasks.Task StopAsync(CancellationToken cancellationToken)
        {
            // _logger.LogInformation("CronJob 3 is stopping.");
            return base.StopAsync(cancellationToken);
        }
    }
}