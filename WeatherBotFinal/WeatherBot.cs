using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using Newtonsoft.Json;

namespace WeatherBotFinal
{
    // Classes imported from OpenWeatherMap Api JSON response
    public class Rootobject
    {
        public Coord coord { get; set; }
        public Weather[] weather { get; set; }
        public string _base { get; set; }
        public Main main { get; set; }
        public int visibility { get; set; }
        public Wind wind { get; set; }
        public Clouds clouds { get; set; }
        public int dt { get; set; }
        public Sys sys { get; set; }
        public int id { get; set; }
        public string name { get; set; }
        public int cod { get; set; }
    }

    public class Coord
    {
        public float lon { get; set; }
        public float lat { get; set; }
    }

    public class Main
    {
        public float temp { get; set; }
        public double pressure { get; set; }
        public int humidity { get; set; }
        public float temp_min { get; set; }
        public float temp_max { get; set; }
    }

    public class Wind
    {
        public float speed { get; set; }
        public double deg { get; set; }
    }

    public class Clouds
    {
        public int all { get; set; }
    }

    public class Sys
    {
        public int population { get; set; }
    }

    public class Weather
    {
        public int id { get; set; }
        public string main { get; set; }
        public string description { get; set; }
        public string icon { get; set; }
    }

    public class Forecast
    {
        public List[] list { get; set; }

    }

    public class List
    {
        public int dt { get; set; }
        public Main main { get; set; }
        public Weather[] weather { get; set; }
        public Clouds clouds { get; set; }
        public Wind wind { get; set; }
        public Rain rain { get; set; }
        public Sys1 sys { get; set; }
        public string dt_txt { get; set; }
    }

    public class Sys1
    {
        public string pod { get; set; }
    }

    public class Rain
    {
        public float _3h { get; set; }
    }


    public class WeatherBot
    {
        /// <summary>
        /// Gets the current weather information for the given city
        /// </summary>
        /// <param name="city">city name eg."Seattle"</param>
        /// <returns>Object containing weather information</returns>
        public static async Task<Rootobject> GetWeather(string city)
        {
            string strRet = string.Empty;
            if (string.IsNullOrEmpty(city))
            {
                // strRet = "City name given is not valid... Or something went wrong.";
                return null;
            }


            string sURL;
            sURL = "WeatherAppUrl" + (city) +
                   "AppID";

            using (var client = new HttpClient())
            {
                HttpResponseMessage msg = await client.GetAsync(sURL);

                if (msg.IsSuccessStatusCode)
                {
                    var jsonResponse = await msg.Content.ReadAsStringAsync();
                    var _data = JsonConvert.DeserializeObject<Rootobject>(jsonResponse);
                    return _data;
                }
                return null;
            }
        }

        /// <summary>
        /// Gets weather description
        /// </summary>
        /// <param name="WeatherMapEnv">environment name returned from WeatherMap Api</param>
        /// <returns>Weather description</returns>
        public static async Task<string> GetDescription(string WeatherMapEnv)
        {
            StringBuilder sb = new StringBuilder();

            switch (WeatherMapEnv)
            {
                case "Rain":
                    sb.AppendLine();
                    sb.Append("\n\nIts going to be rainy! Fetch an umbrella!");
                    break;

                case "Clear":
                    sb.Append("\nIts going to be sunny! Plan your barbecue!");
                    break;

                case "Clouds":
                    sb.Append("\n Its going to be cloudy! Watch a game!");
                    break;

                default:
                    sb.Append("No description available.");
                    break;
            }

            return sb.ToString();
        }

        /// <summary>
        /// Gets weather forecast for the given city
        /// </summary>
        /// <param name="city">city name eg."Seattle"</param>
        /// <returns>Forecast object containing weather information</returns>
        public static async Task<Forecast> GetForecast(string city)
        {
            if (string.IsNullOrEmpty(city))
            {
                return null;
            }

            Forecast retForecast = new Forecast();
            string sURL;
            sURL = "WeatherAppUrl" + (city) +
                   "appid";

            using (var client = new HttpClient())
            {
                HttpResponseMessage msg = await client.GetAsync(sURL);

                if (msg.IsSuccessStatusCode)
                {
                    var jsonResponse = await msg.Content.ReadAsStringAsync();
                    JObject results = JObject.Parse(jsonResponse);

                    int i = 0;
                    retForecast.list = new List[50];

                    foreach (var result in results["list"])
                    {
                        // Create objects to hold forecast information accordingly
                        List _data = new List();
                        _data.main = new Main();
                        _data.weather = new Weather[5];
                        _data.weather[0] = new Weather();
                        retForecast.list[i] = new List();

                        // Forecast temperature
                        var temperature = result["main"]["temp"];
                        temperature = (float)temperature * 9 / 5 - 459.67;

                        // Forecast date stamp
                        var dt_stamp = result["dt_txt"];

                        // Weather environment (rainy, cloudy, etc)
                        var environment = result["weather"].First["main"];

                        // Assign values to the forecast object 
                        _data.main.temp = (float)temperature;
                        _data.dt_txt = dt_stamp.ToString();
                        _data.weather[0].main = environment.ToString();

                        retForecast.list[i] = _data;
                        i++;

                    }
                }
                else
                {
                    return null;
                }
            }
            return retForecast;
        }
    }
}