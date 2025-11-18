using CommunityToolkit.Mvvm.ComponentModel;

namespace AquaPP.Models;

public enum MessageSender
{
    User,
    Agent
}

public partial class ChatMessage : ObservableObject
{
    [ObservableProperty] private string? _content;

    [ObservableProperty] private bool _isLoading;
    
    public MessageSender Sender { get; set; }

    public bool IsUser => Sender == MessageSender.User;
}