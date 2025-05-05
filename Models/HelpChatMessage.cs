using Microsoft.Extensions.AI;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace HelpChat.Models;

public enum MessageRole
{
    User,
    Assistant,
    System
}

public enum MessageSource
{
    LocalSLM,
    AzureOpenAI,
    SystemGenerated
}

public class HelpChatMessage : INotifyPropertyChanged
{
    private string _content = "...";
    private bool _isThinking;
    private bool _hasFallbackOption;
    private MessageRole _role;

    public MessageRole Role
    {
        get => _role;
        set => SetProperty(ref _role, value);
    }

    public string Content
    {
        get => _content;
        set => SetProperty(ref _content, value);
    }

    public MessageSource Source { get; set; }

    public bool IsThinking
    {
        get => _isThinking;
        set => SetProperty(ref _isThinking, value);
    }

    public DateTime Timestamp { get; set; } = DateTime.Now;

    public event PropertyChangedEventHandler? PropertyChanged;

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }
}