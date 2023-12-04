using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ApothecaryShared;  // Adjust this using statement to match your project's structure

[Route("api/[controller]")]
[ApiController]
public class PrescriptionController : ControllerBase
{
    //private readonly IHttpClientFactory _httpClientFactory;

    //public PrescriptionController(IHttpClientFactory httpClientFactory)
    //{
    //    _httpClientFactory = httpClientFactory;
    //}

    [HttpPost("extract-prescription")]
    public async Task<IActionResult> ExtractPrescription([FromBody] PrescriptionImageDataModel imageData)
    {
        var httpClient = new HttpClient();
        var content = new StringContent(JsonConvert.SerializeObject(imageData), Encoding.UTF8, "application/json");
        //Add your Power Automate URL here,below URL is a private URL where you will not have access
        var powerAutomateUrl = "https://1prod-166.westus.logic.azure.com:443/workflows/63594d31def647dc8f6bf6b3a8600977/triggers/manual/paths/invoke?api-version=2016-06-01&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=vL5NP0sNkE0hr_Nq38vJWPTkOba7fbOIPsRgRzvMVbw"; // Replace with your Power Automate URL

        try
        {
            var response = await httpClient.PostAsync(powerAutomateUrl, content);

            if (response.IsSuccessStatusCode)
            {
                var extractedData = await response.Content.ReadAsStringAsync();
                var medicineOrders = JsonConvert.DeserializeObject<List<MedicineOrder>>(extractedData);
                return Ok(medicineOrders);
            }
            else
            {
                var errorResponse = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, errorResponse);
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, "An error occurred while processing the prescription: " + ex.Message);
        }
    }
}
