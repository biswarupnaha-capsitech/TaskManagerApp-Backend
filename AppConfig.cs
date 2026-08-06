using Capsitech.Data.MongoDB;
using Capsitech.Storage;
using Capsitech.Utility;
using MongoDB.Bson;
using System.Text.RegularExpressions;

namespace TaskManager
{
    public class AppConfig
    {
        public string Version { get; set; }

        public static AppConfig Current { get; protected set; } = new AppConfig();
        public string? AdminEmail { get; set; }

        public string? DeveloperEmail { get; set; }
        public string? HREmail { get; set; }

        public string? AppUrl { get; set; }

        public string? IPStackKey { get; set; }
        public string? IPGeoLocationKey { get; set; }

        public AppConfigJwt? Jwt { get; set; }
        public DBConfiguration DBConfig { get; set; } = new DBConfiguration();
        public StorageConfiguration? StorageConfig { get; set; }

        public AppConfigStorageNames? Storage { get; set; }

        public BlobAccountFolder? BlobAccountFolder { get; set; }
        public string? Container_Profile { get; set; }
        public string? Biometric_Device_Url { get; set; }

        public string GetAppUrl(string urlFor) => $"{Current?.AppUrl?.TrimEnd('/')}/{urlFor?.TrimStart('/')}";

        public static void Init(IConfiguration configuration)
        {
            try
            {
                AppConfig appConfig = configuration.GetSection("AppConfig")
                                                   .Get<AppConfig>() ?? new AppConfig();
                appConfig.DBConfig = configuration.GetSection("DBConfiguration")
                                                  .Get<DBConfiguration>() ?? new DBConfiguration();
                appConfig.StorageConfig = configuration.GetSection("StorageConfiguration")
                                                       .Get<StorageConfiguration>() ?? new StorageConfiguration();
                appConfig.Jwt = configuration.GetSection("Jwt").Get<AppConfigJwt>() ?? new AppConfigJwt();
                appConfig.Container_Profile = configuration["AppConfig:Container_Profile"] ?? "";

                appConfig.AdminEmail = configuration["AppConfig:AdminEmail"] ?? "";
                appConfig.BlobAccountFolder = configuration.GetSection("BlobAccountFolder").Get<BlobAccountFolder>() ?? new BlobAccountFolder();

                Current = appConfig;
            }
            catch (Exception)
            {
                Current = new AppConfig();
            }
        }

        public static void SetCurrentConfig(AppConfig appConfig) => Current = appConfig;


        public static List<FYOptions> GetFYOptions(int startedYear)
        {
            List<FYOptions> response = new List<FYOptions>();
            int startYear = startedYear;
            if (startYear <= 0) startYear = startedYear;
            for (int i = DateTime.Today.Year + 1; i >= startYear; i--)
            {
                string FirstYear = ConvertUtility.ToString(i - 1);
                string SecondYear = ConvertUtility.ToString(i);
                var result = new FYOptions
                {
                    Value = i,
                    AY = FirstYear + "-" + SecondYear,
                    IsDefaultYear = i == DateTime.Today.Year + 1 ? true : false
                };
                response.Add(result);
            }
            return response;
        }
        public static int GetFNYear(int currentYear, int currentMonth)
        {
            int fyear = currentYear;

            int[] fnmonth = { 0, 1, 2, 3 };
            bool IsExist = Array.Exists(fnmonth, m => m == currentMonth);
            fyear = IsExist ? currentYear : (currentYear + 1);
            return fyear;
        }
        public static int GetFNMonth(int currentMonth)
        {
            int fmonth = currentMonth;
            int[] months = { 0, 4, 5, 6, 7, 8, 9, 10, 11, 12, 1, 2, 3 };
            var mindex = Array.IndexOf(months, currentMonth);
            fmonth = mindex;
            return fmonth;
        }
        // extra method
        public static BsonRegularExpression GetBsonRegEx(string value, bool withWordBoundary = false)
        {
            value = Regex.Replace(value, "[^0-9a-z\\+@\\._\\/\\-\\&()?,\\[\\] ]+", " ", RegexOptions.IgnoreCase)?.Replace("+", "\\+")?.Replace("(", "\\(")?.Replace(")", "\\)")?.Replace("/", "\\/")?.Replace("?", "\\?")?.Replace("[", "\\[")?.Replace("]", "\\]")?.Trim();
            return new BsonRegularExpression(withWordBoundary ? ("\\b" + value + "\\b") : value, "i");
        }
        public static DateTime ConvertDateTimeIntoUTC(DateTime datetime, string timeZone, DateTime? date = null)
        {
            // If a date is provided, combine it with the datetime
            if (date.HasValue)
            {
                datetime = new DateTime(date.Value.Year, date.Value.Month, date.Value.Day, datetime.Hour, datetime.Minute, datetime.Second, datetime.Millisecond);
            }

            // Ensure datetime is treated as 'Unspecified' (not UTC or Local)
            datetime = DateTime.SpecifyKind(datetime, DateTimeKind.Unspecified);

            // Convert to UTC based on the provided timezone
            return TimeZoneInfo.ConvertTimeToUtc(datetime, TimeZoneInfo.FindSystemTimeZoneById(timeZone));
        }

    }

    public class BlobAccountFolder
    {
        public string? Container_Name { get; set; }
    }

    public class AppConfigJwt
    {
        public string? Key { get; set; }
        public string? Issuer { get; set; }
    }

    public class AppConfigStorageNames
    {
        public string Docs { get; set; } = "TaskManager-uat-docs";

    }
    public class FYOptions
    {
        public int Value { get; set; }
        public bool IsDefaultYear { get; set; }
        public string? AY { get; set; }
    }
}
