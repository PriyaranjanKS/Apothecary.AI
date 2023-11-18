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
        public IActionResult Post([FromBody] RegistrationModel registrationModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            // TODO: Add logic to process registration data
            // This might involve saving data to a database, calling external APIs, etc.

            try
            {
                // Perform registration logic here

                string json = JsonConvert.SerializeObject(registrationModel);
                // If successful, return a success response
                // If an error occurs, throw an exception or return an error response
                // For simplicity, we assume registration is successful here
                return Ok(new { message = "Registration successful" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration");
                // Handle the error and return an appropriate error response
                return StatusCode(500, new { message = "Registration failed" });
            }
        }



        /*public async Task<IActionResult> UploadImage([FromForm] IFormFile file)
         {
             if (file == null || file.Length == 0)
             {
                 return BadRequest("No file uploaded");
             }

             try
             {
                 // Logic to send the file to Power Automate and get the extracted information
                 // Assuming you have a method that does this and returns a RegistrationModel
                 //RegistrationModel registrationModel = new RegistrationModel();
                  RegistrationModel registrationModel = await ProcessImageThroughPowerAutomate(file);
                 return Ok(registrationModel);
             }
             catch (Exception ex)
             {
                 _logger.LogError(ex, "Error during image processing");
                 return StatusCode(500, new { message = "Error processing image" });
             }
         }
        */

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


        /*
        private async Task<RegistrationModel> ProcessImageThroughPowerAutomate(IFormFile file)
        {
            using var httpClient = new HttpClient();
            using var content = new MultipartFormDataContent();
            using var fileStream = file.OpenReadStream();

            content.Add(new StreamContent(fileStream), "file", file.FileName);

            // Replace the URL with your Power Automate flow endpoint
            var response = await httpClient.PostAsync("https://prod-51.westus.logic.azure.com:443/workflows/778cd526998a4e33942c61c936429532/triggers/manual/paths/invoke?api-version=2016-06-01&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=pzhTvzAeNajFjpTHBiMQhTZ0Nr2OZyKe12go6-q8rs8", content);
                          
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Error calling Power Automate");
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var registrationModel = JsonConvert.DeserializeObject<RegistrationModel>(jsonResponse);

            return registrationModel;
        }
        */
    }
}
