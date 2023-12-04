using Microsoft.AspNetCore.Mvc;
using ApothecaryShared; // Replace with your shared project's namespace
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;
namespace ApothecaryServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegistrationController : ControllerBase
    {

        private readonly ILogger<RegistrationController> _logger; // Add a logger

        public RegistrationController(ILogger<RegistrationController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] RegistrationModel registrationModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Serialize the registration model to JSON
                string json = JsonConvert.SerializeObject(registrationModel);

                // Send the JSON to Power Automate
                var response = await SendDataToPowerAutomate(json);

                // Check response and return accordingly
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return Ok(new { message = "Registration successful", details = responseContent });
                }
                else
                {
                    return StatusCode((int)response.StatusCode, new { message = "Registration failed" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration");
                return StatusCode(500, new { message = "Registration failed" });
            }
        }

        private async Task<HttpResponseMessage> SendDataToPowerAutomate(string jsonData)
        {
            using var httpClient = new HttpClient();
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            // Power Automate endpoint
            string powerAutomateUrl = "https://prod-141.westus.logic.azure.com:443/workflows/20a19631190e4c69a459ea468e5bac82/triggers/manual/paths/invoke?api-version=2016-06-01&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=k7n54hgE__z8PKEJzsFS75lfYDKhj8thlIxEjdo3yJA";

            try
            {
                return await httpClient.PostAsync(powerAutomateUrl, content);
            }
            catch (Exception ex)
            {
                // Log the exception (consider using a logging framework)
                _logger.LogError(ex, "Error occurred while sending data to Power Automate.");

                // Return an HttpResponseMessage indicating failure
                return new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.InternalServerError,
                    Content = new StringContent($"Error occurred while communicating with Power Automate: {ex.Message}")
                };
            }
        }


        [HttpPost("uploadImage")]
        public async Task<IActionResult> UploadImage([FromBody] ImageUploadModel imageData)
        {
            if (imageData == null || string.IsNullOrWhiteSpace(imageData.Content))
            {
                return BadRequest("No image data provided");
            }

            try
            {
                RegistrationModel registrationModel = await ProcessImageThroughPowerAutomate(imageData);
                return Ok(registrationModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during image processing");
                return StatusCode(500, new { message = "Error processing image" });
            }
        }


        private async Task<RegistrationModel> ProcessImageThroughPowerAutomate(ImageUploadModel imageData)
        {
            using var httpClient = new HttpClient();
            var jsonContent = JsonConvert.SerializeObject(imageData);

            var response = await httpClient.PostAsync(
                "https://prod-51.westus.logic.azure.com:443/workflows/778cd526998a4e33942c61c936429532/triggers/manual/paths/invoke?api-version=2016-06-01&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=pzhTvzAeNajFjpTHBiMQhTZ0Nr2OZyKe12go6-q8rs8",
                new StringContent(jsonContent, Encoding.UTF8, "application/json"));

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Error calling Power Automate");
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<RegistrationModel>(jsonResponse);
        }


       
    }
}
