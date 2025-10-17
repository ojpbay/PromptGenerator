using Azure;
using Azure.AI.OpenAI;
using OpenAI.Chat;
using System.Windows;

namespace PromptContextGenerator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            var endpoint = new Uri("https://semantickernel-resource.cognitiveservices.azure.com/");
            var deploymentName = "gpt-4.1-mini";
            var apiKey = "<your-api-key>";

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