using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;
using System.IO;
using System.Windows;

namespace PromptContextGenerator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IConfiguration _configuration;
        private readonly AzureOpenAISettings _azureOpenAISettings;

        public MainWindow()
        {
            InitializeComponent();
            
            // Build configuration with local settings taking precedence
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true);
            
            _configuration = builder.Build();
            
            // Bind Azure OpenAI settings
            _azureOpenAISettings = new AzureOpenAISettings();
            _configuration.GetSection("AzureOpenAI").Bind(_azureOpenAISettings);
        }

        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            var endpoint = new Uri(_azureOpenAISettings.Endpoint);
            var deploymentName = _azureOpenAISettings.DeploymentName;
            var apiKey = _azureOpenAISettings.ApiKey;

            AzureOpenAIClient azureClient = new(
                endpoint,
                new AzureKeyCredential(apiKey));
            ChatClient chatClient = azureClient.GetChatClient(deploymentName);

            var requestOptions = new ChatCompletionOptions()
            {
                Temperature = 0.7f,
                TopP = 1.0f,
                FrequencyPenalty = 0.0f,
                PresencePenalty = 0.0f,

            };

            List<ChatMessage> messages =
            [
                new SystemChatMessage("You are a helpful assistant."),
                new UserChatMessage("I am going to Paris, what should I see?"),
            ];

            var response = chatClient.CompleteChat(messages, requestOptions);
            System.Console.WriteLine(response.Value.Content[0].Text);
        }
    }
}