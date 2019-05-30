#r "Newtonsoft.Json"

using Microsoft.Azure.CognitiveServices.Language.TextAnalytics;
using Microsoft.Azure.CognitiveServices.Language.TextAnalytics.Models;
using System.Collections.Generic;
using Microsoft.Rest;
using System.Net.Http;

using System;
using System.Net;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Configuration;

using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

class TextAnalyticsProcessing
{
    /// <summary>
    /// Container for subscription credentials. Make sure to enter your valid key.
    private TraceWriter log;
    private ITextAnalyticsClient client;

    /// </summary>
    class ApiKeyServiceClientCredentials : ServiceClientCredentials
    {
        public override Task ProcessHttpRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.Headers.Add("Ocp-Apim-Subscription-Key", ConfigurationManager.AppSettings["TextAnalyticsSubKey"]);
            return base.ProcessHttpRequestAsync(request, cancellationToken);
        }
    }

    //Translator Objects
    class TranslatedResult
    {
        public List<Translations> Translations { get; set; }
        public Object detectedLanguage { get; set; }
    }

    class Translations
    {
        public string Text { get; set; }
        public string to { get; set; }
    }
    
    public TextAnalyticsProcessing(TraceWriter Log)
    {
        log = Log;
        log.Info("Text Processing Section Called");
        // Create a client.
        client = new TextAnalyticsClient(new ApiKeyServiceClientCredentials())
        {
            Endpoint = "https://australiaeast.api.cognitive.microsoft.com"
        }; //Replace 'australiaeast' with the correct region for your Text Analytics subscription
        log.Info("Text Processing section initialized");
    }

    public string DetectLanguageFromText(string text)
    {
        log.Info($"Got Text {text}");
        string detectedLanguage = "";
        var result = client.DetectLanguageAsync(new BatchInput(
                    new List<Input>()
                        {
						//// Below are some example texts.. Only for testing
                        //   new Input("1", "This is a document written in English."),
                        //   new Input("2", "Este es un document escrito en Español."),
                        //   new Input("3", "这是一个用中文写的文件"),
                          new Input("1", text)
                    })).Result;

        // Printing language results.
        foreach (var document in result.Documents)
        {
            detectedLanguage = document.DetectedLanguages[0].Iso6391Name;
            log.Info($"Document ID: {document.Id} , Language: {document.DetectedLanguages[0].Name}");
        }
        return detectedLanguage;
    }

    public string getTranslatedText(string text)
    {
        string host = "https://api.cognitive.microsofttranslator.com";
        string route = "/translate?api-version=3.0&to=en";
        string translatedText = "";

        System.Object[] body = new System.Object[] { new { Text = text } };
        var requestBody = JsonConvert.SerializeObject(body);

        using (var client = new HttpClient())
        using (var request = new HttpRequestMessage())
        {
            // Set the method to POST
            request.Method = HttpMethod.Post;

            // Construct the full URI
            request.RequestUri = new Uri(host + route);

            // Add the serialized JSON object to your request
            request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");

            // Add the authorization header
            request.Headers.Add("Ocp-Apim-Subscription-Key", ConfigurationManager.AppSettings["TranslatorTextSubKey"]);

            // Send request, get response
            var response = client.SendAsync(request).Result;
            var jsonResponse = response.Content.ReadAsStringAsync().Result;

            // Print the response
            log.Info(jsonResponse);
            JArray result = JArray.Parse(jsonResponse);
            log.Info(result[0].ToString());
            TranslatedResult translatorObj = JsonConvert.DeserializeObject<TranslatedResult>(result[0].ToString());
            foreach(var translate in translatorObj.Translations)
            {
                log.Info($"Translate text {translate.Text}");
                translatedText = translate.Text;
            }
        }
        return translatedText;
    }
}