using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace WeatherBotFinal
{
    [Serializable]
    public class MyBot : IDialog
    {
        async Task IDialog<object>.StartAsync(IDialogContext context)
        {
            context.Call<TopChoice>(new FormDialog<TopChoice>(new TopChoice()), WhatDoYouWant);
        }

        public async Task WhatDoYouWant(IDialogContext context, IAwaitable<TopChoice> choices)
        {
            switch ((await choices).Choice.Value)
            {
                case TopChoices.PredictWeather:
                    context.Call(
                        new PromptDialog.PromptString("Enter the city name:", "Please enter a valid city name:", 3), TellForecast);
                    break;
                case TopChoices.CurrentWeather:
                    context.Call(
                        new PromptDialog.PromptString("Enter the city name:", "Please enter a valid city name:", 3), TellWeather);
                    break;

                default:
                    await context.PostAsync("I don't understand");
                    context.Call<TopChoice>(
                        new FormDialog<TopChoice>(new TopChoice(), options: FormOptions.PromptInStart),
                        WhatDoYouWant);
                    break;
            }
        }

        public async Task TellForecast(IDialogContext context, IAwaitable<string> city)
        {
            Forecast forecast = await WeatherBot.GetForecast(await city);
            StringBuilder sb = new StringBuilder();

            if (null != forecast)
            {
                int k = 0;
                foreach (var listItem in forecast.list)
                {
                    if (k < 10)
                    {
                        sb.Append(string.Format("Temperature at {0} will be {1}.\n", listItem.dt_txt,
                            Math.Round(listItem.main.temp, 2)));
                        k++;
                    }
                    else
                        break;
                }

                sb.AppendLine(await WeatherBot.GetDescription(forecast.list[0].weather[0].main));

                context.UserData.SetValue("LastCity", await city);
            }
            else
            {
                sb.AppendLine("Cityname entered is invalid or something went wrong.");
            }

            // Reply to the user
            await context.PostAsync(sb.ToString());

            context.Done<object>(null);
            // context.Call<TopChoice>(new FormDialog<TopChoice>(new TopChoice(), options: FormOptions.PromptInStart), WhatDoYouWant);
        }

        public async Task TellWeather(IDialogContext context, IAwaitable<string> city)
        {
            Rootobject weather = await WeatherBot.GetWeather(await city);
            string message = string.Empty;

            if (null != weather)
            {
                // Convert temperature from kelvin to fahrenheit
                message = string.Format("Temperature in {0} is {1} degree Fahrenheit.\n", await city,
                    Math.Round(weather.main.temp * 9 / 5 - 459.67, 2));

                // Get description like "its going to be cloudy or rainy"
                message = message + "\n\n" + (await
                    WeatherBot.GetDescription(weather.weather[0].main));

                context.UserData.SetValue("LastCity", await city);
            }
            else
            {
                message = "Cityname entered is invalid or something went wrong.";
            }

            // Reply to the user
            await context.PostAsync(message);

            context.Done<object>(null);
            // context.Call<TopChoice>(new FormDialog<TopChoice>(new TopChoice(), options: FormOptions.PromptInStart), WhatDoYouWant);
        }
    }

    public enum TopChoices
    {
        PredictWeather,
        CurrentWeather
    }

    [Serializable]
    public class TopChoice
    {
        public TopChoices? Choice;
    }

}