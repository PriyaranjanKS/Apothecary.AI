using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

[Route("api/[controller]")]
[ApiController]
public class ImageProcessingController : ControllerBase
{
    [HttpPost("extracttext")]
    public async Task<IActionResult> ExtractTextFromImage([FromBody] ImageDataModel model)
    {
        var base64Image = model.Base64Image;
        // Replace with your Power Automate URL
        var powerAutomateUrl = "https://prod-68.westus.logic.azure.com:443/workflows/1462ab643dd843b087c8c8bacc0a500f/triggers/manual/paths/invoke?api-version=2016-06-01&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=yXSGiauL0Vxl6jkifj3lGCXI43wP_D8lMxR6iOhNFts";

        try
        {
            var httpClient = new HttpClient();
            var content = new StringContent(JsonConvert.SerializeObject(new { base64Image }), Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync(powerAutomateUrl, content);

            if (response.IsSuccessStatusCode)
            {
                var extractedText = await response.Content.ReadAsStringAsync();
                // Assume we have a method to parse the response and extract the information needed
                var medicineInfo = ParseExtractedText(extractedText);
                return Ok(medicineInfo.MedicineName);
            }
            else
            {
                // You can log the error message for debugging purposes
                var errorResponse = await response.Content.ReadAsStringAsync();
                // Return the error message with the status code from the response
                return StatusCode((int)response.StatusCode, errorResponse);
            }
        }
        catch (Exception ex)
        {
            // Log the exception message
            // You could log this to a file or a database
            // For example: LogError(ex.Message);

            // Return a generic error message to the client
            // You might want to return a more specific message depending on the context
            return StatusCode(500, "An error occurred while processing the image.");
        }


    }
    private MedicineInfo ParseExtractedText(string jsonText)
    {
        // Deserialize the JSON string to a C# object
        // You'll need to define a MedicineInfo class that matches the structure of your JSON
        var medicineInfo = JsonConvert.DeserializeObject<MedicineInfo>(jsonText);
        return medicineInfo;
    }

    // Define a class to match the JSON structure returned from Power Automate
    public class MedicineInfo
    {
        public string MedicineName { get; set; }
      
    }
    public class ImageDataModel
    {
        public string Base64Image { get; set; }
    }
}
