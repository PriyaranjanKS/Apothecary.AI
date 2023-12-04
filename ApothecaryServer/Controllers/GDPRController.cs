using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class GdprController : ControllerBase
{
    // POST: api/gdpr/data-compliance
    [HttpPost("data-compliance")]
    public async Task<IActionResult> DataCompliance([FromBody] GdprRequest request)
    {
        // Replace with your actual Power Automate URL
        var powerAutomateUrl = "https://1prod-03.westus.logic.azure.com:443/workflows/22edabac660b4f5d851052dc1a95df7a/triggers/manual/paths/invoke?api-version=2016-06-01&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=ZNp_EK3PrtniIkR-Dge5e0O_ktdUx1UsN_5TMIGRh1k";

        try
        {
            using var httpClient = new HttpClient();
            var jsonContent = JsonConvert.SerializeObject(request);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(powerAutomateUrl, content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                // Here you can deserialize the response content if needed
                return Ok(responseContent);
            }
            else
            {
                return StatusCode((int)response.StatusCode, "Failed to process GDPR action.");
            }
        }
        catch (HttpRequestException httpEx)
        {
            // Handle HTTP request exceptions
            return StatusCode(500, $"HTTP Request failed: {httpEx.Message}");
        }
        catch (Exception ex)
        {
            // Handle any other exceptions
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }

    public class GdprRequest
    {
        public string UserName { get; set; }
        public string ActionToBeDone { get; set; }
    }
}