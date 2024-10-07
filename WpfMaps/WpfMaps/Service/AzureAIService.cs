namespace WpfMaps
{
    using Azure.AI.OpenAI;
    using Azure;
    using System;
    using System.Threading.Tasks;
    using System.Windows.Media.Imaging;
    using System.Windows.Media;

    /// <summary>
    /// Provides access to the Azure AI service for performing various AI operations, including chat completions and image-related tasks.
    /// </summary>
    public class AzureAIService
    {
        const string endpoint = "";
        const string deploymentName = "GPT-4O";
        string key = "";

        OpenAIClient? azureClient;
        ChatCompletionsOptions? chatCompletions;

        public AzureAIService()
        {
            if (!(string.IsNullOrEmpty(endpoint) && string.IsNullOrEmpty(key)))
            {
                this.AzureClient = new OpenAIClient(new Uri(endpoint), new AzureKeyCredential(key));
            }
        }

        /// <summary>
        /// Get or Set Azure OpenAI client.
        /// </summary>
        public OpenAIClient? AzureClient
        {
            get
            {
                return azureClient;
            }
            set
            {
                azureClient = value;
            }
        }

        /// <summary>
        /// Retrieves an answer from the deployment name model using the provided user prompt.
        /// </summary>
        /// <param name="userPrompt">The user prompt.</param>
        /// <returns>The AI response.</returns>
        internal async Task<string> GetResponseFromOpenAI(string userPrompt)
        {
            this.chatCompletions = new ChatCompletionsOptions
            {
                DeploymentName = deploymentName,
                Temperature = (float)0.5,
                MaxTokens = 800,
                NucleusSamplingFactor = (float)0.95,
                FrequencyPenalty = 0,
                PresencePenalty = 0,
            };

            if (this.AzureClient != null)
            {
                // Add the user's prompt as a user message to the conversation.
                this.chatCompletions?.Messages.Add(new ChatRequestSystemMessage("You are a predictive analytics assistant."));

                // Add the user's prompt as a user message to the conversation.
                this.chatCompletions?.Messages.Add(new ChatRequestUserMessage(userPrompt));
                try
                {
                    // Send the chat completion request to the OpenAI API and await the response.
                    var response = await this.AzureClient.GetChatCompletionsAsync(this.chatCompletions);

                    // Return the content of the first choice in the response, which contains the AI's answer.
                    return response.Value.Choices[0].Message.Content;
                }
                catch
                {
                    // If an exception occurs (e.g., network issues, API errors), return an empty string.
                    return "";
                }
            }

            return "";
        }

        /// <summary>
        /// Method to get the image from AI.
        /// </summary>
        /// <param name="locationName"> The location name</param>
        /// <returns>The bitmap image</returns>
        public async Task<ImageSource> GetImageFromAI(string locationName)
        {
            var imageGenerations = await AzureClient!.GetImageGenerationsAsync(
                new ImageGenerationOptions()
                {
                    Prompt = $"Share the {locationName} image.",
                    Size = ImageSize.Size1024x1024,
                    Quality = ImageGenerationQuality.Standard,
                    DeploymentName = "DALL-E",

                });

            Uri imageUri = imageGenerations.Value.Data[0].Url;
            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.UriSource = imageUri;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();
            return bitmapImage;
        }
    }
}