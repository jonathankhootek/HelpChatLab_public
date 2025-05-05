using HelpChat.Models;
using HelpChat.Utils;
using Microsoft.Extensions.AI;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HelpChat.Services;

public class ChatService
{
    private readonly List<HelpChatMessage> _conversationHistory = new();
    private readonly IChatClient _chatClient;

    public Microsoft.UI.Dispatching.DispatcherQueue WindowDispatcherQueue { get; internal set; }

    public ChatService()
    {
        _chatClient = Task.Run(() => PhiSilicaClient.CreateAsync()).Result;
    }

    public async Task WelcomeUser(HelpChatMessage welcomeMessage)
    {
        Log.Information("Generating welcome message");
        await Task.Run(
           async () =>
           {
               //TODO Insert 1.3 below


               WindowDispatcherQueue.TryEnqueue(() => welcomeMessage.IsThinking = true);

               string fullResult = "";
               await foreach (var partialResult in _chatClient.GetStreamingResponseAsync(
                                  [
                                    new ChatMessage(
                         ChatRole.User,
                         FileUtils.GetFileContents("Prompts\\Welcome.txt")),
                   ],
                                  null))
               {
                   fullResult += partialResult;

                   WindowDispatcherQueue.TryEnqueue(() => welcomeMessage.Content = fullResult);
               }

               WindowDispatcherQueue.TryEnqueue(() => welcomeMessage.IsThinking = false);

               //TODO 1.4 Edit prompt in Welcome.txt


               // Insert above

           });
    }

    public async Task GetChatResponseAsync(string prompt, HelpChatMessage outputMessage, CancellationToken cancellationToken)
    {
        //TODO Insert 10.2 below

        Log.Information($"🧠 Generating Phi Silica response to '{prompt}'");

        outputMessage.IsThinking = true;
        outputMessage.Content = "⏳🤔";

        string fullResult = "";

        await Task.Run(
           async () =>
           {
               await foreach (var partialResult in _chatClient.GetStreamingResponseAsync(
                   [
                       new ChatMessage(ChatRole.User, prompt)
                   ], null, cancellationToken))
               {
                   fullResult += partialResult;

                   WindowDispatcherQueue.TryEnqueue(() => outputMessage.Content = fullResult);

                   if (cancellationToken.IsCancellationRequested) break;
               }
           }, cancellationToken);

        outputMessage.IsThinking = false;

        //Insert above
    }
    public async Task CallAzure(HelpChatMessage outputMessage, string prompt)
    {
        //TODO Insert 4.4 below

        Log.Information($"🧠 Generating Azure OpenAI response to '{prompt}'");

        outputMessage.IsThinking = true;
        outputMessage.Content = "⏳🤔";

        string fullResult = "";

        var azureOpenAIService = new AzureOpenAIService();
        var chatUpdates = azureOpenAIService.Client.CompleteChatStreamingAsync([new OpenAI.Chat.UserChatMessage(prompt)]);

        await foreach (var chatUpdate in chatUpdates)
        {
            foreach (var contentPart in chatUpdate.ContentUpdate)
            {
                fullResult += contentPart.Text;
            }

            WindowDispatcherQueue.TryEnqueue(() => outputMessage.Content = fullResult);
        }

        outputMessage.IsThinking = false;

        //Insert above
    }
}