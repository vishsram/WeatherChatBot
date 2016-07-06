using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Utilities;
using Newtonsoft.Json;
using Microsoft.Bot.Builder.Dialogs;
using System.Text;

namespace WeatherBotFinal
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<Message> Post([FromBody]Message message)
        {
            if (message.Type == "Message")
            {
                bool setCity = false;
                WeatherLUIS weatherLuis = await LUISWeatherClient.ParseUserInput(message.Text);
                string strRet = string.Empty;
                string strWeather = message.Text;


                if (null != weatherLuis && weatherLuis.intents.Count() > 0)
                {
                    switch (weatherLuis.intents[0].intent)
                    {
                        case "None":
                            return await Conversation.SendAsync(message, () => new MyBot());

                        case "repeat": // For messages without city name
                            strWeather = message.GetBotUserData<string>("LastCity");
                            if (null == strWeather)
                            {
                                strRet = "Previous city is not available.";
                            }
                            else
                            {
                                Rootobject temperature = await WeatherBot.GetWeather(strWeather);

                                // Convert temperature from kelvin to fahrenheit
                                strRet = string.Format("Temperature in {0} is {1} degree Fahrenheit.\n", strWeather,
                                    Math.Round(temperature.main.temp * 9 / 5 - 459.67, 2));

                                // Get description like "its going to be cloudy or rainy"
                                strRet = strRet + "\n\n" + (await
                                    WeatherBot.GetDescription(temperature.weather[0].main));
                            }
                            break;

                        case "weather": // Current weather for given city
                            if (weatherLuis.entities.Length > 0 && !string.IsNullOrEmpty(weatherLuis.entities[0].entity))
                            {
                                if (weatherLuis.entities[0].type == "city" || weatherLuis.entities[0].type == "builtin.geography.city")
                                {
                                    setCity = true;

                                    Rootobject temperature = await WeatherBot.GetWeather(weatherLuis.entities[0].entity);
                                    strRet = string.Format("Temperature in {0} is {1} degree Fahrenheit.\n", weatherLuis.entities[0].entity,
                                        Math.Round(temperature.main.temp * 9 / 5 - 459.67));

                                    // Get description like "its going to be cloudy or rainy"
                                    strRet = strRet + "\n\n" + (await
                                        WeatherBot.GetDescription(temperature.weather[0].main));
                                }
                                else
                                {
                                    strRet = "Please enter a valid city name.";
                                }

                            }
                            else
                            {
                                strRet = "Please enter a valid city name.";
                            }
                            break;

                        case "forecast": // Weather forecast for 10 three hour period for given city
                            if (weatherLuis.entities.Length > 0 && !string.IsNullOrEmpty(weatherLuis.entities[0].entity) &&
                                    weatherLuis.entities[0].type == "city")
                            {
                                setCity = true;
                                Forecast temperature = await WeatherBot.GetForecast(weatherLuis.entities[0].entity);
                                StringBuilder sb = new StringBuilder();
                                int k = 0;
                                foreach (var listItem in temperature.list)
                                {
                                    if (k < 10)
                                    {
                                        sb.Append(string.Format("Temperature at {0} will be {1}.\n", listItem.dt_txt,
                                            Math.Round(listItem.main.temp)));
                                        k++;
                                    }
                                    else
                                        break;
                                }

                                sb.AppendLine(await WeatherBot.GetDescription(temperature.list[0].weather[0].main));

                                strRet = sb.ToString();
                            }

                            else
                            {
                                // If city name is not given for forecast, use the last used city
                                strWeather = message.GetBotUserData<string>("LastCity");
                                if (null == strWeather)
                                {
                                    strRet = "Previous city is not available.";
                                }
                                else
                                {
                                    Forecast temperature = await WeatherBot.GetForecast(strWeather);
                                    StringBuilder sb = new StringBuilder();
                                    sb.Append(string.Format("Weather forecast for {0}: {1} degree Fahrenheit.",
                                        strWeather,
                                        Math.Round(temperature.list[0].main.temp)));


                                    sb.AppendLine(await
                                        WeatherBot.GetDescription(
                                            temperature.list[0].weather[0].main));


                                    strRet = sb.ToString();
                                }
                            }

                            break;

                        default:
                            strRet = "Sorry, I didn't understand.";
                            break;

                    }
                }
                else
                {
                    strRet = "Sorry, I didn't understand.";
                }

                Message ReplyMessage = message.CreateReplyMessage(strRet);

                if (setCity && !string.IsNullOrEmpty(weatherLuis.entities[0].entity))
                {
                    ReplyMessage.SetBotUserData("LastCity", weatherLuis.entities[0].entity);
                }

                return ReplyMessage;


            }
            else
            {
                return HandleSystemMessage(message);
            }
        }

        private Message HandleSystemMessage(Message message)
        {
            if (message.Type == "Ping")
            {
                Message reply = message.CreateReplyMessage();
                reply.Type = "Ping";
                return reply;
            }
            else if (message.Type == "DeleteUserData")
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == "BotAddedToConversation")
            {
            }
            else if (message.Type == "BotRemovedFromConversation")
            {
            }
            else if (message.Type == "UserAddedToConversation")
            {
            }
            else if (message.Type == "UserRemovedFromConversation")
            {
            }
            else if (message.Type == "EndOfConversation")
            {
            }

            return null;
        }
    }
}