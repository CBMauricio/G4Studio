using RestSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using Windows.ApplicationModel.Resources;

namespace G4Studio.Utils
{
    public class TimeZoneReferenceTime
    {
        public string Tag { get; set; }
        public TimeSpan StandardOffset { get; set; }
        public TimeSpan DaylightSavings { get; set; }
        public DateTime WallTime { get; set; }

        public int PosixTzValidYear { get; set; }

        public string PosixTz { get; set; }

        public DateTime Sunrise { get; set; }
        public DateTime Sunset { get; set; }
    }

    public class TimeZoneNames
    {
        public string ISO6391LanguageCode { get; set; }
        public string Generic { get; set; }
        public string Standard { get; set; }
        public string Daylight { get; set; }
    }

    public class TimeZone
    {
        public string Id { get; set; }
        public TimeZoneNames Names { get; private set; }
        public TimeZoneReferenceTime ReferenceTime { get; private set; }
    }

    public class TimeZoneObj
    {
        public string Version { get; set; }
        public string ReferenceUtcTimestamp { get; set; }

        public List<TimeZone> TimeZones { get; private set; }
    }

    public static class TimeZoneHandler
    {
        private static ResourceLoader resourceLoader;
        public static TimeZoneObj GetTimeZone(double latitude, double longitude, DateTime date)
        {
            /* Brecht: uncomment code above */
            
            /*
            var client = new RestClient("https://atlas.microsoft.com/timezone/byCoordinates/json");
            var request = new RestRequest(
                string.Format(CultureInfo.InvariantCulture, "?api-version=1.0&query={0}&subscription-key={1}&timeStamp={2}",
                    string.Format(CultureInfo.InvariantCulture, "{0},{1}", latitude, longitude),
                    resourceLoader.GetString("eYRSmPQSVrCEKjoxnQQK4f0La_5pmzOucRq2ac7lFak"),
                    date.ToString("u", CultureInfo.InvariantCulture)),
                Method.GET);

            var response = client.Execute<TimeZoneObj>(request);

            return response.Data;
            */


            /* Brecht: comment code above */
            resourceLoader = ResourceLoader.GetForViewIndependentUse();

            var client = new RestClient(resourceLoader.GetString("CONFS_Azure_Atlas_RESTClient"));
            var request = new RestRequest(
                string.Format(CultureInfo.InvariantCulture, resourceLoader.GetString("CONFS_Azure_Atlas_RESTRequest"), 
                    string.Format(CultureInfo.InvariantCulture, "{0},{1}" , latitude, longitude), 
                    resourceLoader.GetString("CONFS_Azure_Atlas_Key"),
                    date.ToString("u", CultureInfo.InvariantCulture)), 
                Method.GET);
            
            var response = client.Execute<TimeZoneObj>(request); 

            return response.Data;
        }

        public static DateTime Sunrise(double latitude, double longitude, DateTime date)
        {
            TimeZoneObj timeZoneObj = GetTimeZone(latitude, longitude, date);

            if (timeZoneObj.TimeZones.Count > 0)
            {
                return timeZoneObj.TimeZones[0].ReferenceTime.Sunrise.Add(timeZoneObj.TimeZones[0].ReferenceTime.StandardOffset);
            }

            return new DateTime();
        }

        public static DateTime Sunset(double latitude, double longitude, DateTime date)
        {
            TimeZoneObj timeZoneObj = GetTimeZone(latitude, longitude, date);

            if (timeZoneObj.TimeZones.Count > 0)
            {
                return timeZoneObj.TimeZones[0].ReferenceTime.Sunset.Add(timeZoneObj.TimeZones[0].ReferenceTime.StandardOffset);
            }

            return new DateTime();
        }
    }
}
