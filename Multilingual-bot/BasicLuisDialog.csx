using System;
using System.Configuration;
using System.Threading.Tasks;
using System.Net;
using System.Threading;
using Newtonsoft.Json;

using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;

// For more information about this template visit http://aka.ms/azurebots-csharp-luis
[Serializable]
public class BasicLuisDialog : LuisDialog<object>
{
     private const string EntityDevice = "Device";
     private const string EntityOperation = "Operation";
     private const string EntityRoom = "Room";

    public BasicLuisDialog() : base(new LuisService(new LuisModelAttribute(
        ConfigurationManager.AppSettings["LuisAppId"], 
        ConfigurationManager.AppSettings["LuisAPIKey"], 
        domain: ConfigurationManager.AppSettings["LuisAPIHostName"])))
    {
    }

    [LuisIntent("None")]
    public async Task NoneIntent(IDialogContext context, LuisResult result)
    {
        await this.ShowLuisResult(context, result);
    }

    [LuisIntent("Other")]
    public async Task OtherIntent(IDialogContext context, LuisResult result)
    {
        await this.ShowLuisResult(context, result);
    }

    // Go to https://luis.ai and create a new intent, then train/publish your luis app.
    // Finally replace "Greeting" with the name of your newly created intent in the following handler
    [LuisIntent("Greeting")]
    public async Task GreetingIntent(IDialogContext context, LuisResult result)
    {
        await context.PostAsync($"Hi, welcome to your Home bot.. How can I help");
        context.Wait(MessageReceived);
    }

    [LuisIntent("TurnOff")]
    public async Task TurnOffIntent(IDialogContext context, LuisResult result)
    {
        await this.ShowLuisResult(context, result);
    }

    [LuisIntent("Dim")]
    public async Task DimIntent(IDialogContext context, LuisResult result)
    {
        await this.ShowLuisResult(context, result);
    }

    [LuisIntent("TurnOn")]
    public async Task TurnOnIntent(IDialogContext context, LuisResult result)
    {
        string device = "";
        string operation = "";
        string room = "";
        string reply = "";

        await context.PostAsync($"Found Turn On Intent. Trying to get entities for {result.Query}");

        EntityRecommendation devicRecommend;
        EntityRecommendation operationRecommend;
        EntityRecommendation roomRecommend;

        if (result.TryFindEntity(EntityDevice, out devicRecommend))
        {
            device = devicRecommend.Entity;
        }
        if (result.TryFindEntity(EntityOperation, out operationRecommend))
        {
            operation = operationRecommend.Entity;
        }
        if (result.TryFindEntity(EntityRoom, out roomRecommend))
        {
            room = roomRecommend.Entity;
        }
        
        if(device != "")
        {
            if(room != "")
            {
                reply = $"Sure, Turning on {device} in {room}";
            }
            else
            {
                reply = $"Sure, Turning on {device}";
            }
        }

        await context.PostAsync($"{reply}...");
        context.Wait(MessageReceived);
    }

    [LuisIntent("Help")]
    public async Task HelpIntent(IDialogContext context, LuisResult result)
    {
         await context.PostAsync("Hi! Try asking me things like 'Turn on lights' or 'Turn off bedroom lights'");
        context.Wait(this.MessageReceived);
    }

    private async Task ShowLuisResult(IDialogContext context, LuisResult result) 
    {
        await context.PostAsync($"Sorry I don't know what to do for this query. You have reached {result.Intents[0].Intent}. You said: {result.Query}");
        context.Wait(MessageReceived);
    }
}

