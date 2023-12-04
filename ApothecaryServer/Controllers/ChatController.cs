using Microsoft.AspNetCore.Mvc;
using ApothecaryShared; // Replace with your shared project's namespace
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;
using Azure.AI.OpenAI;
using DotNetEnv;
using static System.Environment;
using Azure;
using Microsoft.Rest;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Identity.Client;
namespace ApothecaryServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : Controller
    {
        string endpoint = "https://openaihackthon.openai.azure.com/";
        string openAIkey = "b98b579efd7e41518645fb27f7650671";
        string openAImodel = "DeenuGPT";

        static string productdvcall;
        static string url = "https://org2b36f581.crm.dynamics.com/";
        // TODO Specify the Dataverse environment name to connect with.
        // See https://learn.microsoft.com/power-apps/developer/data-platform/webapi/compose-http-requests-handle-errors#web-api-url-and-versions


        // Microsoft Entra ID app registration shared by all Power App samples.
        string clientId = "a2c1a069-b173-4b32-82b6-fd319e729fb2";
        string redirectUri = "http://localhost"; // Loopback for the interactive login.


        private readonly ILogger<RegistrationController> _logger; // Add a logger

        public ChatController(ILogger<RegistrationController> logger)
        {
            _logger = logger;
        }


        [HttpGet("getallproducts")]
        public string GetAllProducts()
        {
            //Add your Power Automate URL here,below URL is a private URL where you will not have access
            var powerAutomateUrl = "https://1prod-30.westus.logic.azure.com:443/workflows/088c0841dbe04743994f0a16d880c2b2/triggers/manual/paths/invoke?api-version=2016-06-01&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=4EOMBAVYzpoHQk8N8eYEuDiGiJQYPdQObheBmNeiZyY"; // Replace with your actual Power Automate URL

            try
            {
                using var httpClient = new HttpClient();
                // Create an empty content for the POST request
                var content = new StringContent("", System.Text.Encoding.UTF8, "application/json");

                var response = httpClient.PostAsync(powerAutomateUrl, content).Result; // Blocking call

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = response.Content.ReadAsStringAsync().Result; // Blocking call                  
                    productdvcall = responseContent;
                    _logger.LogError(productdvcall, "Error during registration");
                    return productdvcall;
                }
                else
                {
                    var errorContent = response.Content.ReadAsStringAsync().Result; // Blocking call
                    return "";
                }
            }
            catch (Exception ex)
            {
                return "";
            }
        }


        [HttpPost("GetOpenAIResponse")]
        public async Task<IActionResult> GetResponse(string usermessage)
        {
            try
            {
                OpenAIClient client = new OpenAIClient(new Uri(endpoint), new AzureKeyCredential(openAIkey));
                var chatcompleteionsoptions = new ChatCompletionsOptions()
                {
                    Messages =
        {
            new ChatMessage(ChatRole.System,"You are a helpful assistant"),
            new ChatMessage(ChatRole.User,"The following information is from the json text but don't show full product info to user"+GetAllProducts()),
            //new ChatMessage(ChatRole.Assistant,"Yes it does"),
            new ChatMessage(ChatRole.User,usermessage)
        },
                    MaxTokens = 1000
                };

                Response<ChatCompletions> response = await client.GetChatCompletionsAsync(deploymentOrModelName: openAImodel , chatcompleteionsoptions);
                var botresponse = response.Value.Choices.First().Message.Content;
                return Json(new { Response = botresponse });
            }
            catch (Exception ex)
            {
                // Handle the exception here, you can log it or return an error response
                return Json(new { Error = $"An error occurred: {ex.Message}" });
            }

        }



    }
}
