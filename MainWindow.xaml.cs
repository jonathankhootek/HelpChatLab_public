using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.Extensions.DependencyInjection;
using HelpChat.Services;
using HelpChat.ViewModels;
using HelpChat.Models;
using System.Threading.Tasks;
using System;
using Microsoft.UI.Windowing;
using Microsoft.UI;
using Serilog.Events;
using Serilog.Sinks.WinUi3.LogViewModels;
using Serilog.Templates;
using Serilog;
using Serilog.Sinks.WinUi3;
using HelpChat.Utils;
using Microsoft.Extensions.AI;
using System.Threading;
using System.Text;

namespace HelpChat;

public sealed partial class MainWindow : Window
{
    public ChatViewModel ViewModel { get; }
    private readonly ChatService _chatService;


    private CancellationTokenSource _cancellationTokenSource = new();

    private AppData _appData = new AppData();
    private ItemsRepeaterLogBroker _logBroker;

    private double _maxConversationHeight = 0;

    public MainWindow(ServiceProvider serviceProvider)
    {
        InitializeComponent();


        _chatService = serviceProvider.GetRequiredService<ChatService>();
        _chatService.WindowDispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();

        ViewModel = new ChatViewModel(_chatService);

        AppWindow.SetIcon("icon.ico");

        //Set up logging
        _logBroker = new ItemsRepeaterLogBroker(LogViewer, LogScrollViewer,
            new EmojiLogViewModelBuilder(defaultForeground: Colors.Black)
                .SetTimestampFormat(new ExpressionTemplate("[{@t:HH:mm:ss.fff}]"))
                .SetMessageFormat(new ExpressionTemplate("{@m}"))
                );

        Log.Logger = new LoggerConfiguration()
            .WriteTo.WinUi3Control(_logBroker)
            .CreateLogger();
        Log.Information("App started");

        this.Closed += (s, e) =>
        {
            if (_cancellationTokenSource != null) _cancellationTokenSource.Cancel();
        };

    }

    /// <summary>
    /// Once everything loads, have Phi Silica generate a message to welcome the user
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void Grid_Loaded(object sender, RoutedEventArgs e)
    {
        //TODO Insert 1.1 below

        await ViewModel.AddWelcomeMessage();

        //Insert above

    }

    /// <summary>
    /// Get an answer to the chat question when input is submitted
    /// </summary>
    /// <returns></returns>
    private async Task SendMessage()
    {
        //TODO Insert 2.1 below

        //Don't do anything with an empty message
        string messageText = MessageTextBox.Text.Trim();
        if (string.IsNullOrEmpty(messageText))
            return;

        //Don't do anything if there are messages in progress
        foreach (var message in ViewModel.Messages) if (message.IsThinking) return;

        Log.Information($"❓ {messageText}");

        //Clear the message textbox
        MessageTextBox.Text = "";

        await ViewModel.AddNewUserMessage(messageText, _cancellationTokenSource);

        //Insert above
    }


    private async void SendButton_Click(object sender, RoutedEventArgs e)
    {
        //TODO Insert 2.2 below

        await SendMessage();

        //Insert above
    }

    private async void MessageTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        //TODO Insert 2.3 below

        if (e.Key == Windows.System.VirtualKey.Enter)
        {
            e.Handled = true;
            await SendMessage();
        }

        //Insert above
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource = new CancellationTokenSource(); // Reset for future use
    }


    private async void LogButton_Click(object sender, RoutedEventArgs e)
    {
        LogOverlay.Visibility = Visibility.Visible;
        await Task.Delay(50);
        LogScrollViewer.ChangeView(null, LogScrollViewer.ScrollableHeight, null);
    }



    private void CloseLogOverlay(object sender, RoutedEventArgs e)
    {
        LogOverlay.Visibility = Visibility.Collapsed;
    }

    /// <summary>
    /// Scroll to the bottom when we get more text from the language models
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ChatItemsRepeaterControl_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (e.NewSize.Height > _maxConversationHeight && ViewModel.IsChatThinking)
        {
            _maxConversationHeight = e.NewSize.Height;
            ChatScrollViewer.ScrollToVerticalOffset(((ChatItemsRepeaterControl)sender).ActualHeight);
        }
    }
}