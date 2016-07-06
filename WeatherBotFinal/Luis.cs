using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;

namespace WeatherBotFinal
{
    // Classes imported from LUIS json response
    public class WeatherLUIS
    {
        public string query { get; set; }
        public Intent[] intents { get; set; }
        public Entity[] entities { get; set; }
    }

    public class Intent
    {
        public string intent { get; set; }
        public float score { get; set; }
    }

    public class Entity
    {
        public string entity { get; set; }
        public string type { get; set; }
        public int startIndex { get; set; }
        public int endIndex { get; set; }
        public Resolution resolution { get; set; }
        public float score { get; set; }
    }

    public class Resolution
    {
        public string time { get; set; }
    }


    public class LUISWeatherClient
    {
        /// <summary>
        /// Parse the user message 
        /// </summary>
        /// <param name="strInput">User input</param>
        /// <returns>WeatherLUIS object containing intents(weather or forecast) and entities (city, environment, etc)</returns>
        public static async Task<WeatherLUIS> ParseUserInput(string strInput)
        {
            string strRet = string.Empty;
            string strEscaped = Uri.EscapeDataString(strInput);

            using (var client = new HttpClient())
            {
                string uri = "Your Luis app Url" + strEscaped;
                HttpResponseMessage msg = await client.GetAsync(uri);

                if (msg.IsSuccessStatusCode)
                {
                    var jsonResponse = await msg.Content.ReadAsStringAsync();
                    var _Data = JsonConvert.DeserializeObject<WeatherLUIS>(jsonResponse);
                    return _Data;
                }
                else { /* wrong info */ }
            }
            return null;
        }
    }
}