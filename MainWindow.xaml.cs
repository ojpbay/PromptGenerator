using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;
using System.IO;
using System.Windows;
using System.Collections.Generic;

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

        private async void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            // Get user input
            var userMessage = userInput.Text.Trim();
            if (string.IsNullOrEmpty(userMessage))
            {
                MessageBox.Show("Please enter a message.", "Input Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Disable the generate button while processing
                GenerateButton.IsEnabled = false;
                GenerateButton.Content = "Generating...";

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
                    new UserChatMessage(userMessage),
                ];

                var response = await chatClient.CompleteChatAsync(messages, requestOptions);
                var responseText = response.Value.Content[0].Text;

                // Append to results textbox
                if (!string.IsNullOrEmpty(results.Text))
                {
                    results.Text += "\n\n";
                }
                results.Text += $"User: {userMessage}\n\nAssistant: {responseText}";
                
                // Scroll to bottom
                results.ScrollToEnd();

                // Clear the input textbox for next input
                userInput.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                // Re-enable the generate button
                GenerateButton.IsEnabled = true;
                GenerateButton.Content = "Generate";
                
                // Focus back to input textbox
                userInput.Focus();
            }
        }
    }
}