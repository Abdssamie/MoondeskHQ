using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.SemanticKernel.ChatCompletion;
using Splat;
using ILogger = Serilog.ILogger;
using Microsoft.SemanticKernel;

namespace AquaPP.AI;

#pragma warning disable SKEXP0070

interface IAgent
{
    public Task<ChatMessageContent> SendMessage(string message);
}

public class Agent : IAgent
{
    private readonly Kernel _kernel;
    private readonly IChatCompletionService _chatCompletionService; // Use the interface
    private readonly ChatHistory _history = [];
    private readonly ILogger _logger;


    // Constructor to initialize Kernel and ChatCompletionService
    public Agent(ILogger logger)
    {
        _logger = logger;


        var googleApiKey = Environment.GetEnvironmentVariable("GOOGLE_API_KEY");

        if (string.IsNullOrEmpty(googleApiKey))
        {
            _logger.Error("Google API Key not found. Please set the GOOGLE_API_KEY environment variable.");
            throw new InvalidOperationException(
                "Google API Key not found. Please set the GOOGLE_API_KEY environment variable.");
        }

        var geminiOpenAiCompatibleEndpoint =
            "https://generativelanguage.googleapis.com/v1beta/openai/"; // This is the key!
        var modelId = "gemini-2.5-flash";

        // 1. Create a KernelBuilder
        var kernelBuilder = Kernel.CreateBuilder()
            .AddOpenAIChatCompletion(
                modelId: modelId,
                apiKey: googleApiKey,
                // For Azure OpenAI, you'd use serviceId, endpoint, and deploymentName.
                // For raw OpenAI compatibility, you just need base_url and apiKey.
                // Semantic Kernel's AddOpenAIChatCompletion often has overloads to handle this.
                // You might need to directly instantiate OpenAIClient and pass it in for full control.
                httpClient: new HttpClient() { BaseAddress = new Uri(geminiOpenAiCompatibleEndpoint) }
            );
        _logger.Information("Created the new custom kernel builder successfully");


        _logger.Information("Added GeminiChatCompletion to kernel builder");

        // 3. Build the Kernel
        _kernel = kernelBuilder.Build();
        _logger.Information("Building the kernel");

        // 4. Get the Chat Completion Service from the built Kernel
        // It's best to retrieve the service from the Kernel itself.
        _chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();
    }

    public async Task<ChatMessageContent> SendMessage(string message)
    {
        // Add the user's message to the history
        _history.AddUserMessage(message); // Use the 'message' parameter, not hardcoded "Hello, how are you?"

        // Invoke the chat completion service with the history and the kernel
        try
        {
            var stopwatch = Stopwatch.StartNew();

            var response = await _chatCompletionService.GetChatMessageContentsAsync(
                _history,
                kernel: _kernel // Pass the kernel instance
            );
            
            stopwatch.Stop();
            
            _logger.Information($"Code block executed in {stopwatch.ElapsedMilliseconds} ms.");

            // Ensure there's at least one response before returning
            if (response.Count > 0)
            {
                _history.AddAssistantMessage(response[0].ToString());
                return response[0];
            }
            
            _logger.Warning("Received an empty response from the Gemini chat completion model.");
            throw new InvalidOperationException("Empty response received from the AI model.");
        }
        catch (Exception ex)
        {
            _logger.Error($"Error when sending message to Gemini chat completion model: {ex}");
            throw new Exception("There was a problem getting the streamed response from the semantic kernel agent");
        }
    }
}