using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class AlternateMedicineController : ControllerBase
{
    [HttpPost("get-suggestion")]
    public async Task<IActionResult> GetAlternateMedicineSuggestion([FromBody] MedicineRequest request)
    {
        try
        {
            // URL for your Power Automate flow
            var powerAutomateUrl = "https://prod-157.westus.logic.azure.com:443/workflows/3ac1c29ce2ba4a0a94495e67fe8ab5dd/triggers/manual/paths/invoke?api-version=2016-06-01&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=UoisvOwr44B3M5avWQnzFRbtQ_2SxNFc72QVCb6dXhg";

            // Create a new HttpClient instance
            using var httpClient = new HttpClient();

            var requestData = new
            {
                MedicineName = request.MedicineName
            };

            var content = new StringContent(JsonConvert.SerializeObject(requestData), Encoding.UTF8, "application/json");

            // Sending a POST request to the Power Automate flow
            var response = await httpClient.PostAsync(powerAutomateUrl, content);


            if (response.IsSuccessStatusCode)
            {
                // Read the response content and deserialize it
                var responseContent = await response.Content.ReadAsStringAsync();
               var alternateMedicineResponse = JsonConvert.DeserializeObject<AlternateMedicineResponse>(responseContent);

                // Modify the response object to include DALL-EImageURL
                var responseWithImageUrl = new
                {
                    AlternateMedicine = alternateMedicineResponse.AlternateMedicine,
                    DALL_EImageURL = alternateMedicineResponse.DALL_EImageURL
                };

                return Ok(responseWithImageUrl);
            }
            else
            {
                // If the response is not successful, return an error message
                return StatusCode((int)response.StatusCode, "Failed to fetch the alternate medicine suggestion.");
            }
        }
        catch (Exception ex)
        {
            // If an exception occurs, return a 500 status code with the exception message
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }

    // Helper class for the response
    public class MedicineRequest
    {
        public string MedicineName { get; set; }
    }
    private class AlternateMedicineResponse
    {
        public string AlternateMedicine { get; set; }
        public string DALL_EImageURL { get; set; }
    }
}
