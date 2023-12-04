using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class VoiceProcessingController : ControllerBase
{
    [HttpPost("voice")]
    public async Task<IActionResult> Post([FromBody] AudioDataModel audioDataModel)
    {
        string base64Audio = audioDataModel.AudioData;
        // Replace with your actual Power Automate flow URL and any necessary headers or parameters
        string powerAutomateUrl = "https://1prod-32.westus.logic.azure.com:443/workflows/1e9e2521746b461da0e26f73903f2b84/triggers/manual/paths/invoke?api-version=2016-06-01&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=g4CdOor_S-yUciuLL1P_uaA-ENKZIN5CNkRrf5LG0hM";
        var httpClient = new HttpClient();

        try
        {
            var content = new StringContent(base64Audio, Encoding.UTF8, "application/octet-stream");

            // Add headers if needed, e.g. httpClient.DefaultRequestHeaders.Authorization = ...

            HttpResponseMessage response = await httpClient.PostAsync(powerAutomateUrl, content);

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var voiceOrderResponse = JsonConvert.DeserializeObject<VoiceOrderResponse>(jsonResponse);

                // Parse the Order JSON string into an object
                var orders = JsonConvert.DeserializeObject<List<MedicineOrder>>(voiceOrderResponse.Order);

                // Do something with the orders, like adding to a database or processing further

                return Ok(new { voiceOrderResponse.Translation, Orders = orders });
            }
            else
            {
                // Log the error or handle it as needed
                return StatusCode((int)response.StatusCode, response.ReasonPhrase);
            }
        }
        catch (HttpRequestException httpRequestEx)
        {
            // Handle exception
            return StatusCode(500, httpRequestEx.Message);
        }
        finally
        {
            httpClient.Dispose();
        }
    }
}
public class AudioDataModel
{
    public string AudioData { get; set; }
}
public class VoiceOrderResponse
{
    [JsonProperty("Translation")]
    public string Translation { get; set; }

    [JsonProperty("Order")]
    public string Order { get; set; } // This is a JSON string
}

public class MedicineOrder
{
    [JsonProperty("ProductName")]
    public string ProductName { get; set; }

    [JsonProperty("MedicationDuration")]
    public decimal MedicationDuration { get; set; }

    [JsonProperty("Quantity")]
    public decimal Quantity { get; set; }

    [JsonProperty("TotalPrice")]
    public decimal TotalPrice { get; set; }
}
