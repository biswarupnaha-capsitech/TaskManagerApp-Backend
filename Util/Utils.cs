// Ignore Spelling: Util Utils

using Capsitech.Data.MongoDB;
using Capsitech.Extensions;
using Projects.Common;
using Projects.Models;
using MongoDB.Bson;

namespace Projects.Util
{
    public class Utils
    {
        static public DateTime StringToTime(string time, string errorMessage, DateTime date)
        {
            DateTime today;
            if (date != default)
                today = new DateTime(date.Year, date.Month, date.Day,0,0,0,0,DateTimeKind.Utc);
            else
                throw new Exception($"Invalid date {date}");

            //today = IndianDateTime.ConvertToIndianTime(today);
            var timeParts = time.Split(':') ?? throw new Exception(errorMessage);

            if (timeParts.Length < 2)
            {
                throw new Exception($"Invalid time format: {time}");
            }

            var hh = timeParts[0];
            var mm = timeParts[1][..2]; //subString(0,2)  Extracting minutes
            var abb = timeParts[1][2..]; //subString(2) Extracting AM/PM

            if (double.TryParse(hh, out double Ihh) && double.TryParse(mm, out double Imm))
            {
                if (abb.ToLower().Trim() == "pm" && Ihh < 12) // Adjust for PM if necessary             
                    Ihh += 12;

                today = today.AddHours(Ihh).AddMinutes(Imm);
            }
            else
            {
                throw new Exception($"Not able to parse {time} to Today's Time");
            }

            return today;
        }

        static public DateTime String24ToTime(string time, DateTime date)
        {
            // Ensure the input date is valid
            if (date == default)
                throw new Exception($"Invalid date {date}");

            // Start with the provided date and reset the time part
            DateTime today = new (date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Utc);

            // Split the time string into components
            var timeParts = time.Split(':') ?? throw new Exception("invalid time format");

            if (timeParts.Length < 2)
            {
                throw new Exception($"Invalid time format: {time}");
            }

            // Parse hours and minutes from the time string
            if (int.TryParse(timeParts[0], out int hours) && int.TryParse(timeParts[1], out int minutes))
            {
                // Validate that hours and minutes are within the correct ranges
                if (hours < 0 || hours > 23 || minutes < 0 || minutes > 59)
                {
                    throw new Exception($"Invalid time value: {time}");
                }

                // Add the parsed hours and minutes to the date
                today = today.AddHours(hours).AddMinutes(minutes);
            }
            else
            {
                throw new Exception($"Not able to parse {time} to Today's Time");
            }

            return today;
        }
        //static public DateTime StringToTime12HrsTo24Hrs(string time, string errorMessage, DateTime date)
        //{
        //    DateTime today;
        //    if (date != default)
        //        today = new DateTime(date.Year, date.Month, date.Day);
        //    else
        //        throw new Exception($"Invalid date {date}");
        //    today = ConvertToIndianTime(today);
        //    var timeParts = time.Split(':') ?? throw new Exception(errorMessage);
        //    string hh="00", mm="00", ss="00", abb="";
        //    if (timeParts.Length < 2)
        //    {
        //        throw new Exception($"Invalid time format: {time}");
        //    }
        //    if(timeParts.Length==2) // 10:15 AM/PM then [10][15 AM/PM]
        //    {
        //         hh = timeParts[0];
        //         mm = timeParts[1][..2]; //subString(0,2)  Extracting minutes
        //         abb = timeParts[1][2..]; //subString(2) Extracting AM/PM
        //    }
        //    else if(timeParts.Length==3) // 07:45:50 AM/PM then [07][45][50 AM/PM]
        //    {
        //         hh = timeParts[0];
        //         mm = timeParts[1][..2]; //subString(0,2)  Extracting minutes
        //         ss = timeParts[2][..2]; // subString(0,2) Extracting seconds
        //         abb = timeParts[2][2..]; //subString(2) Extracting AM/PM
        //    }

        //    if (timeParts.Length == 2 && double.TryParse(hh, out double Ihh) && double.TryParse(mm, out double Imm))
        //    {
        //        if (abb.ToLower().Trim() == "pm" && Ihh < 12) // Adjust for PM if necessary             
        //            Ihh += 12;

        //        else if (abb.ToLower().Trim() == "am" && Ihh == 12) // Adjust for 12 AM
        //            Ihh = 0;

        //        today = today.AddHours(Ihh).AddMinutes(Imm);
        //    }
        //    else if (timeParts.Length == 3 && double.TryParse(hh, out double Ihhh) && double.TryParse(mm, out double Immm) && double.TryParse(ss, out double Iss))
        //    {
        //        if (abb.ToLower().Trim() == "pm" && Ihhh < 12) // Adjust for PM if necessary             
        //            Ihhh += 12;

        //        else if (abb.ToLower().Trim() == "am" && Ihhh == 12) // Adjust for 12 AM
        //            Ihhh = 0;

        //        today = today.AddHours(Ihhh).AddMinutes(Immm).AddSeconds(Iss);
        //    }
        //    else
        //    {
        //        throw new Exception($"Not able to parse {time} to Today's Time");
        //    }

        //    return today.AddHours(6).AddMinutes(30);
        //}

        static public double CalculateMinutes(string startTime, string endTime)
        {
            DateTime startDateTime = DateTime.Parse(startTime);
            DateTime endDateTime = DateTime.Parse(endTime);
            if (startDateTime > endDateTime)
            {
                endDateTime = endDateTime.AddDays(1);
            }
            TimeSpan timeDifference = endDateTime - startDateTime;
            double totalMinutes = timeDifference.TotalMinutes;
            return totalMinutes;
        }
        //public static DateTime ConvertToIndianTime(DateTime dateTime)
        //{
        //    // Find the Indian Standard Time zone
        //    TimeZoneInfo istTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        //    // Convert the provided DateTime to Indian Standard Time
        //    return TimeZoneInfo.ConvertTime(dateTime, istTimeZone);
        //}

        //public static bool IsValidOrNull(RecordUpdateInfo info)
        //{
        //    if (info == null)
        //        return false;
        //    else if (info.Date == default)
        //        return false;

        //    return true;    
        //}

        //public static RecordUpdateInfo AssignIST(RecordUpdateInfo info)
        //{
        //    if (info == null)
        //        return null;
        //    else if (info.Date == DateTime.MinValue)
        //        return info;

        //    RecordUpdateInfo i = new()
        //    {
        //        Date = ConvertToIndianTime(info.Date),
        //        UserName = info.UserName,
        //        UserId = info.UserId
        //    };
        //    return i;
        //}

        //public static DateTime GetTime(DateTime date, int hours = 6,int minutes = 30)
        //{
        //    TimeZoneInfo istTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        //    if (date != DateTime.MaxValue && date != DateTime.MinValue)
        //        return TimeZoneInfo.ConvertTimeFromUtc(date, istTimeZone);           
        //    else
        //        return date;
        //}

        public static bool IsValidTimeForOTP(DateTime date)
        {
            var sourceDay = short.Parse(date.ToString("dd"));
            var sourceMonth = short.Parse(date.ToString("MM"));
            var sourceYear = short.Parse(date.ToString("yyyy"));
            var sourceHours = short.Parse(date.ToString("hh")) % 12;
            var sourceMinutes = short.Parse(date.ToString("mm"));
            var sourceSeconds = short.Parse(date.ToString("ss"));

            var dd = short.Parse(DateTime.UtcNow.ToString("dd"));
            var month = short.Parse(DateTime.UtcNow.ToString("MM"));
            var yy = short.Parse(DateTime.UtcNow.ToString("yyyy"));
            var hh = short.Parse(DateTime.UtcNow.ToString("hh")) % 12;
            var mm = short.Parse(DateTime.UtcNow.ToString("mm"));
            var ss = short.Parse(DateTime.UtcNow.ToString("ss"));

            if (yy >= sourceYear)
            {
                if (month >= sourceMonth)
                {
                    if (dd >= sourceDay)
                    {
                        if (sourceHours >= hh)
                        {
                            if (sourceMinutes > mm) return false;

                            if (sourceMinutes >= mm && sourceSeconds > ss) return false;

                            return true;
                        }
                        else
                            return true;
                    }
                }
                else
                {
                    return true;
                }
            }
            return true;

         }


        //public static bool IsTimePeriodEnd(DateTime date)
        //{
        //    var sourceDay = short.Parse(date.ToString("dd"));
        //    var sourceMonth = short.Parse(date.ToString("MM"));
        //    var sourceYear = short.Parse(date.ToString("yyyy"));
        //    var sourceHours = short.Parse(date.ToString("hh")) % 12;
        //    var sourceMinutes = short.Parse(date.ToString("mm"));
        //    var sourceSeconds = short.Parse(date.ToString("ss"));

        //    var dd = short.Parse(DateTime.UtcNow.ToString("dd"));
        //    var month = short.Parse(DateTime.UtcNow.ToString("MM"));
        //    var yy = short.Parse(DateTime.UtcNow.ToString("yyyy"));
        //    var hh = short.Parse(DateTime.UtcNow.ToString("hh")) % 12;
        //    var mm = short.Parse(DateTime.UtcNow.ToString("mm"));
        //    var ss = short.Parse(DateTime.UtcNow.ToString("ss"));

        //    if (yy >= sourceYear)
        //    {
        //        if (month >= sourceMonth)
        //        {
        //            if (dd >= sourceDay)
        //            {
        //                if (sourceHours >= hh)
        //                {
        //                    if (sourceMinutes > mm) return false;

        //                    if (sourceMinutes >= mm && sourceSeconds > ss) return false;

        //                    return true;
        //                }
        //                else
        //                    return true;
        //            }
        //        }
        //        else
        //        {
        //            return true;
        //        }
        //    }
        //    return true;
        //}

        static public bool IsEmptyOrNull(string val)
        {
            if (val == null || val.IsEmpty())
            {
                return true;
            }
            else
            {
                return false;
            }
        }


    public static string ConvertNumbertoWords(long number)
    {
        if (number == 0) return "ZERO";
        if (number < 0) return "minus " + ConvertNumbertoWords(Math.Abs(number));
        string words = "";
        if ((number / 100000) > 0)
        {
            words += ConvertNumbertoWords(number / 10000) + " LAKES ";
            number %= 100000;
        }
        if ((number / 1000) > 0)
        {
            words += ConvertNumbertoWords(number / 1000) + " THOUSAND ";
            number %= 1000;
        }
        if ((number / 100) > 0)
        {
            words += ConvertNumbertoWords(number / 100) + " HUNDRED ";
            number %= 100;
        }
        //if ((number / 10) > 0)  
        //{  
        // words += ConvertNumbertoWords(number / 10) + " RUPEES ";  
        // number %= 10;  
        //}  
        if (number > 0)
        {
            if (words != "") words += "AND ";
            var unitsMap = new[]
            {
            "ZERO", "ONE", "TWO", "THREE", "FOUR", "FIVE", "SIX", "SEVEN", "EIGHT", "NINE", "TEN", "ELEVEN", "TWELVE", "THIRTEEN", "FOURTEEN", "FIFTEEN", "SIXTEEN", "SEVENTEEN", "EIGHTEEN", "NINETEEN"
        };
            var tensMap = new[]
            {
            "ZERO", "TEN", "TWENTY", "THIRTY", "FORTY", "FIFTY", "SIXTY", "SEVENTY", "EIGHTY", "NINETY"
        };
            if (number < 20) words += unitsMap[number];
            else
            {
                words += tensMap[number / 10];
                if ((number % 10) > 0) words += " " + unitsMap[number % 10];
            }
        }
        return words;
    }



        public static string NumberToWords(int number)
        {
            if (number == 0)
                return "zero";
            if (number < 0)
                return "minus " + NumberToWords(Math.Abs(number));
            string words = "";
            if ((number / 1000000000) > 0)
            {
                words += NumberToWords(number / 1000000000) + " Billion ";
                number %= 1000000000;
            }
            if ((number / 10000000) > 0)
            {
                words += NumberToWords(number / 10000000) + " Crore ";
                number %= 10000000;
            }
            if ((number / 1000000) > 0)
            {
                words += NumberToWords(number / 1000000) + " Million ";
                number %= 1000000;
            }
            if ((number / 100000) > 0)
            {
                words += NumberToWords(number / 100000) + " Lakh ";
                number %= 100000;
            }
            if ((number / 1000) > 0)
            {
                words += NumberToWords(number / 1000) + " Thousand ";
                number %= 1000;
            }
            if ((number / 100) > 0)
            {
                words += NumberToWords(number / 100) + " Hundred ";
                number %= 100;
            }
            if (number > 0)
            {
                if (words != "")
                    words += "and ";
                var unitsMap = new[] { "zero", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten", "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen" };
                var tensMap = new[] { "zero", "Ten", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety" };
                if (number < 20)
                    words += unitsMap[number];
                else
                {
                    words += tensMap[number / 10];
                    if ((number % 10) > 0)
                        words += "-" + unitsMap[number % 10];
                }
            }
            return words;
        }

        public static string MinutesToTime(double num)
        {
            int hours = (int)(num / 60);  // Convert total minutes to hours
            int minutes = (int)(num % 60); // Get the remaining minutes
            string seconds = "00";  // Since we're only converting minutes, seconds will always be 0

            return $"{hours:D2}:{minutes:D2}:{seconds}";
        }

        public static BsonRegularExpression GetBsonRegEx(string search)
        {
            return new BsonRegularExpression(search, "i");  // "i" for case-insensitive matching
        }

    }
}
