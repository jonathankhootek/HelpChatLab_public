using Azure;
using Azure.AI.OpenAI;
using OpenAI.Assistants;
using OpenAI.Chat;
using HelpChat.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace HelpChat.Services;

public class AzureOpenAIService : IDisposable
{
    string KEY = App.Current.Resources["AzureOpenAIKey"].ToString();
    string ENDPOINT = App.Current.Resources["AzureOpenAIEndpoint"].ToString();
    string DEPLOYMENT_NAME = App.Current.Resources["AzureOpenAIDeployment"].ToString();

    public ChatClient? Client { get; set; }

    public AzureOpenAIService()
    {
        try
        {
            var openAI = new AzureOpenAIClient(new Uri(ENDPOINT), new AzureKeyCredential(KEY));
            Client = openAI.GetChatClient(DEPLOYMENT_NAME);
        }
        catch (Exception ex)
        {
            // Log error
            Console.WriteLine($"Error initializing Azure OpenAI: {ex}");
            Client = null;
        }
    }

    public void Dispose()
    {
        Client = null;
    }
}