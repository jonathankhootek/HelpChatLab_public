using Microsoft.Extensions.AI;
using Microsoft.Windows.AI.ContentModeration;
using Microsoft.Windows.AI.Generative;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;

namespace HelpChat.Utils;

internal class PhiSilicaClient : IChatClient
{
    // Search Options
    private LanguageModelOptions _languageModelOptions;

    private LanguageModel? _languageModel;
    private LanguageModelContext? _languageModelContext;

    public ChatClientMetadata Metadata { get; }

    private PhiSilicaClient()
    {
        Metadata = new ChatClientMetadata("PhiSilica", new Uri($"file:///PhiSilica"));
        _languageModelOptions = new LanguageModelOptions
        {
            ContentFilterOptions = new ContentFilterOptions
            {
                ImageMaxAllowedSeverityLevel = new ImageContentFilterSeverity(SeverityLevel.Minimum),
                PromptMaxAllowedSeverityLevel = new TextContentFilterSeverity(SeverityLevel.Minimum),
                ResponseMaxAllowedSeverityLevel = new TextContentFilterSeverity(SeverityLevel.Minimum)
            },
            Temperature = (float).4,
            TopK = 50,
            TopP = (float).9,
        };
    }

    public static async Task<PhiSilicaClient?> CreateAsync(CancellationToken cancellationToken = default)
    {
        var phiSilicaClient = new PhiSilicaClient();

        await phiSilicaClient.InitializeAsync(cancellationToken);
        return phiSilicaClient;
    }

    public Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> chatMessages, ChatOptions? options = null, CancellationToken cancellationToken = default) =>
        GetStreamingResponseAsync(chatMessages, options, cancellationToken).ToChatResponseAsync(cancellationToken: cancellationToken);

    public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IEnumerable<ChatMessage> chatMessages, ChatOptions? options = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (_languageModel == null)
        {
            throw new InvalidOperationException("Language model is not loaded.");
        }

        var prompt = GetPrompt(chatMessages);

        string responseId = Guid.NewGuid().ToString("N");

        await foreach (var part in GenerateStreamResponseAsync(prompt, options, cancellationToken))
        {
            yield return new(ChatRole.Assistant, part)
            {
                ResponseId = responseId,
            };
        }
    }

    private string GetPrompt(IEnumerable<ChatMessage> history)
    {
        if (!history.Any())
        {
            return string.Empty;
        }

        string prompt = string.Empty;

        var firstMessage = history.FirstOrDefault();

        _languageModelContext = firstMessage?.Role == ChatRole.System ?
            _languageModel?.CreateContext(firstMessage.Text, new ContentFilterOptions()) :
            _languageModel?.CreateContext();

        for (var i = 0; i < history.Count(); i++)
        {
            var message = history.ElementAt(i);
            if (message.Role == ChatRole.System)
            {
                if (i > 0)
                {
                    throw new ArgumentException("Only first message can be a system message");
                }
            }
            else if (message.Role == ChatRole.User)
            {
                string msgText = message.Text ?? string.Empty;
                prompt += msgText;
            }
            else if (message.Role == ChatRole.Assistant)
            {
                prompt += message.Text;
            }
        }

        return prompt;
    }

    public void Dispose()
    {
        _languageModel?.Dispose();
        _languageModel = null;
    }

    public object? GetService(Type serviceType, object? serviceKey = null)
    {
        return null;
    }

    public static bool IsAvailable()
    {
        try
        {
            return LanguageModel.GetReadyState() == Microsoft.Windows.AI.AIFeatureReadyState.Ready;
        }
        catch
        {
            return false;
        }
    }

    private async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!IsAvailable())
        {
            await LanguageModel.EnsureReadyAsync();
        }

        cancellationToken.ThrowIfCancellationRequested();

        _languageModel = await LanguageModel.CreateAsync();
    }

    public async IAsyncEnumerable<string> GenerateStreamResponseAsync(string prompt, ChatOptions? options = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (_languageModel == null)
        {
            throw new InvalidOperationException("Language model is not loaded.");
        }

        string currentResponse = string.Empty;
        using var newPartEvent = new ManualResetEventSlim(false);

        if (_languageModel.GetUsablePromptLength(_languageModelContext, prompt) > 0)
        {
            IAsyncOperationWithProgress<LanguageModelResponseResult, string>? progress;

            progress = _languageModel.GenerateResponseAsync(_languageModelContext, prompt, _languageModelOptions);


            progress.Progress = (result, value) =>
            {
                currentResponse = value;
                newPartEvent.Set();
                if (cancellationToken.IsCancellationRequested)
                {
                    progress.Cancel();
                }
            };

            while (progress.Status != AsyncStatus.Completed)
            {
                await Task.CompletedTask.ConfigureAwait(ConfigureAwaitOptions.ForceYielding);

                if (newPartEvent.Wait(10, cancellationToken))
                {
                    yield return currentResponse;
                    newPartEvent.Reset();
                }
            }

            var response = await progress;

            yield return response?.Status switch
            {
                LanguageModelResponseStatus.BlockedByPolicy => "\nBlocked by policy",
                LanguageModelResponseStatus.ResponseBlockedByContentModeration => "\nBlocked by content moderation",
                LanguageModelResponseStatus.PromptBlockedByContentModeration => "\nPrompt blocked by content moderation",
                LanguageModelResponseStatus.PromptLargerThanContext => "\nPrompt larger than context",
                _ => string.Empty,
            };
        }
        else
        {
            yield return "Prompt is too large for this model. Please submit a smaller prompt";
        }
    }
}