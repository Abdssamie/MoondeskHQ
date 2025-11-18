using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using AquaPP.AI;
using AquaPP.Models;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.SemanticKernel;
using SukiUI.Toasts;
using ILogger = Serilog.ILogger;

namespace AquaPP.ViewModels.Pages;

public partial class ChatViewModel : PageBase
{
    private readonly Agent? _agent;
    private readonly ILogger _logger;
    private readonly ISukiToastManager _toastService;

    [ObservableProperty] private string _message = string.Empty;

    public Bitmap UserImage { get; }
    public Bitmap AgentImage { get; }

    public ObservableCollection<ChatMessage> Messages { get; }

    /// <summary>
    /// Design time only constructor
    /// </summary>
    #pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public ChatViewModel() : base("Chat", "fa-solid fa-comment")
    {
    }

    public ChatViewModel(ISukiToastManager toastManager, ILogger logger) : base("Chat", "fa-solid fa-comment")
    {
        _logger = logger;
        _toastService = toastManager;

        try
        {
            _agent = new Agent(logger);
        }
        catch (InvalidOperationException ex)
        {
            _logger.Warning(ex, "Agent initialization failed. Chat functionality will be disabled.");
            _agent = null;
        }
        
        Messages = [];
        SendCommand = new AsyncRelayCommand(SendMessage);

        UserImage = new Bitmap(AssetLoader.Open(new Uri("avares://AquaPP/Assets/avatar_image.jpeg")));
        AgentImage =
            new Bitmap(AssetLoader.Open(new Uri("avares://AquaPP/Assets/svg-chatbot-icon--freesvgorg133669.png")));

        _logger.Information("ChatViewModel initialized.");
    }

    public ICommand SendCommand { get; }

    private async Task SendMessage()
    {
        if (string.IsNullOrWhiteSpace(Message))
        {
            return;
        }

        if (_agent == null)
        {
            _toastService.CreateToast()
                .WithTitle("Chat Unavailable")
                .WithContent("Chat functionality is disabled. Please set the GOOGLE_API_KEY environment variable and restart the application.")
                .Dismiss().After(TimeSpan.FromSeconds(5))
                .Dismiss().ByClicking()
                .Queue();
            return;
        }

        var userMessage = new ChatMessage { Content = Message, Sender = MessageSender.User, IsLoading = false };
        Messages.Add(userMessage);

        try
        {
            // Prepares the message to send to the agent
            var messageToSend = Message;
            Message = string.Empty; // Clear the input field

            // Display the circular loading indicator and scroll to bottom
            var newAgentMessageLoading = new ChatMessage
                { Content = string.Empty, Sender = MessageSender.Agent, IsLoading = true };
            Messages.Add(newAgentMessageLoading);

            // Sends the message to the agent
            var agentResponse = await _agent.SendMessage(messageToSend);

            // Simulating streaming of response 
            int chunkSize = 5;
            string fullContent = agentResponse.Content ?? string.Empty;
            string currentDisplayedContent = string.Empty;

            Messages[^1].IsLoading = false;

            // Loop through the string in chunks
            for (var i = 0; i < fullContent.Length; i += chunkSize)
            {
                // Calculate the length of the current chunk, ensuring it doesn't go out of bounds
                int length = Math.Min(chunkSize, fullContent.Length - i);
                string chunk = fullContent.Substring(i, length);

                await Task.Delay(TimeSpan.FromMilliseconds(13));

                // Append the chunk to the current displayed content
                currentDisplayedContent += chunk;

                if (Messages != null && Messages.Any())
                {
                    Messages[^1].Content = currentDisplayedContent;
                }
            }
        }
        catch (HttpOperationException ex)
        {
            _logger.Error($"Error when sending message to Gemini chat completion model: {ex}");
            _toastService.CreateToast()
                .WithTitle("Error Sending Message")
                .WithContent("There was an error sending your message. Please try again.")
                .Dismiss().After(TimeSpan.FromSeconds(3))
                .Dismiss().ByClicking()
                .Queue();
        }
        catch (TaskCanceledException exception)
        {
            _logger.Error($"Error when sending message to Gemini chat completion model: {exception}");
            _toastService.CreateToast()
                .WithTitle("Error Sending Message")
                .WithContent("Agent took too long to respond.")
                .Dismiss().After(TimeSpan.FromSeconds(3))
                .Dismiss().ByClicking()
                .Queue();
        }
        catch (Exception exception)
        {
            _logger.Error($"Error when sending message to Gemini chat completion model: {exception}");
            _toastService.CreateToast()
                .WithTitle("Error Sending Message")
                .WithContent("Unexpected error encountered. Please try again.")
                .Dismiss().After(TimeSpan.FromSeconds(3))
                .Dismiss().ByClicking()
                .Queue();
        }

        Message = string.Empty;
    }
}