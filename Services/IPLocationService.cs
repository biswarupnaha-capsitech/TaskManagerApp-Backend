using Capsitech.Extensions;
using Newtonsoft.Json;

namespace TaskManager.Services
{
    public static class IPLocationService
    {
        public static async Task<IPGeoLocation> GetGeoLocation(string ip)
    {
        IPGeoLocation location = new IPGeoLocation();

        if (!ip.IsEmpty() && ip.IsIPAddress())
        {
            //get details from ipgeolocation.io
            if (!AppConfig.Current.IPGeoLocationKey.IsEmpty())
            {
                try
                {
                    System.Net.WebClient webClient = new System.Net.WebClient();
                    string content = await webClient.DownloadStringTaskAsync($"https://api.ipgeolocation.io/ipgeo?apiKey={AppConfig.Current.IPGeoLocationKey}&ip={ip}&fields=country_name,city,country_code2,isp");
                    if (!content.IsEmpty())
                    {
                        var dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(content);
                        location.CountryCode = dict.ContainsKey("country_code2") ? dict["country_code2"]?.ToString() : null;
                        location.CountryName = dict.ContainsKey("country_name") ? dict["country_name"]?.ToString() : null;
                        location.CityName = dict.ContainsKey("city") ? dict["city"]?.ToString() : null;
                        location.ISP = dict.ContainsKey("isp") ? dict["isp"]?.ToString() : null;
                    }
                }
                catch { }
            }
            //if above method not working (ex. due to exeeded limit)
            //then get details from ipstack.com
            if (!AppConfig.Current.IPStackKey.IsEmpty() && location.CountryCode.IsEmpty() && location.CountryName.IsEmpty())
            {
                try
                {
                    System.Net.WebClient webClient = new System.Net.WebClient();
                    string content = await webClient.DownloadStringTaskAsync($"http://api.ipstack.com/{ip}?access_key={AppConfig.Current.IPStackKey}&format=1");
                    if (!content.IsEmpty())
                    {
                        var dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(content);
                        if (!dict.ContainsKey("error"))
                        {
                            location.CountryCode = dict.ContainsKey("country_code") ? dict["country_code"]?.ToString() : null;
                            location.CountryName = dict.ContainsKey("country_name") ? dict["country_name"]?.ToString() : null;
                            location.CityName = dict.ContainsKey("city") ? dict["city"]?.ToString() : null;
                            location.ISP = dict.ContainsKey("connection") ? (dict["connection"] as dynamic)["isp"]?.ToString() : null;
                        }
                    }
                }
                catch { }
            }
        }

        return location;
    }
}
public class IPGeoLocation
{
    public string CountryCode { get; set; }
    public string CountryName { get; set; }
    public string CityName { get; set; }
    public string ISP { get; set; }
}
}
