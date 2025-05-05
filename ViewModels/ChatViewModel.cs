using HelpChat.AIModels;
using HelpChat.Models;
using HelpChat.Services;
using HelpChat.Utils;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO.Pipelines;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace HelpChat.ViewModels;

public class ChatViewModel : INotifyPropertyChanged, INotifyCollectionChanged
{
    private readonly ChatService _chatService;
    private LocalClassifierService _localClassifierService;

    public ChatViewModel(ChatService chatservice)
    {
        Messages.CollectionChanged += OnMessagesCollectionChanged;
        _chatService = chatservice;

        Task.Run(() => _localClassifierService = new LocalClassifierService());
    }

    public bool IsChatThinking
    {
        get => isChatThinking;
        set
        {
            SetProperty(ref isChatThinking, value);
        }
    }

    public async Task AddWelcomeMessage()
    {
        //TODO Insert 1.2 below

        var welcomeMessage = new HelpChatMessage
        {
            Role = MessageRole.Assistant,
            Source = MessageSource.LocalSLM
        };

        Messages.Add(welcomeMessage);
        await _chatService.WelcomeUser(welcomeMessage);

        //Insert above

    }

    public async Task AddNewUserMessage(string input, CancellationTokenSource cts)
    {
        //TODO Insert 2.4 below

        var userMessage = new HelpChatMessage()
        {
            Role = MessageRole.User,
            Content = input
        };
        Messages.Add(userMessage);

        //Insert above


        //TODO Insert 3.1 below


        IClassifierService classifierService;

        //TODO Insert 8.2a below

        var evaluators = new HelpChat.Evaluators.Evaluators();

        if (evaluators.UseCloud(input)) //Use cloud
        {

            //Insert above

            //Get where to direct the message using cloud ONNX model
            classifierService = new CloudClassifierService();
            var label = await classifierService.GetClassifiedLabel(input, Labels.CandidateLabels);

            //Let user know we used the cloud model
            var helpMessage = new HelpChatMessage
            {
                Role = MessageRole.System,
                Content = $"🔀☁️ Used cloud ONNX model to determine this request is for {label}",
                Source = MessageSource.SystemGenerated
            };
            Messages.Add(helpMessage);

            //Insert above

            //TODO Insert 4.1 below

            //Generate the prompt for the language model
            var prompt = FileUtils.GetFileContents("Prompts\\LanguageModel.txt").Replace("{label}", label).Replace("{input}", input);

            //TODO 4.2 Edit prompt in LanguageModel.txt

            //Insert above

            //TODO Insert 4.3 below

            IsChatThinking = true;


            //Get LLM response
            var newMessage = new HelpChatMessage
            {
                Role = MessageRole.Assistant,
                Content = "",
                Source = MessageSource.AzureOpenAI
            };
            Messages.Add(newMessage);
            await _chatService.CallAzure(newMessage, prompt);

            IsChatThinking = false;

            //Insert above

            //Insert 8.2b below

        }
        else //Use local
        {
            //TODO Insert 9.3 below
            classifierService = _localClassifierService;
            var label = await classifierService.GetClassifiedLabel(input, Labels.CandidateLabels);


            //Let user know we used the local model
            var helpMessage = new HelpChatMessage
            {
                Role = MessageRole.System,
                Content = $"🔀💻 Used local ONNX model to determine this request is for {label}",
                Source = MessageSource.SystemGenerated
            };
            Messages.Add(helpMessage);

            //Insert above

            //TODO Insert 10.1 below

            //Generate the prompt for the language model (same as before)
            var prompt = FileUtils.GetFileContents("Prompts\\LanguageModel.txt").Replace("{label}", label).Replace("{input}", input);

            IsChatThinking = true;

            //Get SLM response
            var newMessage = new HelpChatMessage
            {
                Role = MessageRole.Assistant,
                Content = "",
                Source = MessageSource.LocalSLM
            };
            Messages.Add(newMessage);

            try
            {
                await _chatService.GetChatResponseAsync(prompt, newMessage, cts.Token);
            }
            catch (OperationCanceledException)
            {
                Log.Information("Message sending was canceled.");
            }
            finally
            {
                newMessage.IsThinking = false;
                IsChatThinking = false;
            }

            //Insert above

        }

        //Insert above
    }


    private void OnMessagesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        CollectionChanged?.Invoke(this, e);
    }

    private ObservableCollection<HelpChatMessage> _messages = new();
    private bool isChatThinking;

    public ObservableCollection<HelpChatMessage> Messages
    {
        get => _messages;
        set => SetProperty(ref _messages, value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }
}