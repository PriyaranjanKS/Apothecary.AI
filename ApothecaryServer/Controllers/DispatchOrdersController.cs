using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using static ImageProcessingController;

[Route("api/[controller]")]
[ApiController]
public class DispatchOrdersController : ControllerBase
{

     [HttpPost("schedule")]
    public async Task<ActionResult> ScheduleDispatch([FromBody] DispatchRequest request)
    {
        using var httpClient = new HttpClient();
        var jsonPayload = JsonConvert.SerializeObject(new { OrderId = request.OrderId, CustomerName = request.CustomerName });
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync("https://prod-78.westus.logic.azure.com:443/workflows/a9f7d7ea29604d62a3969b8d9552c514/triggers/manual/paths/invoke?api-version=2016-06-01&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=YR5iYH8a0aG0mMqifX0KadCCHnIlrM41CGKdsRfmYQA", content);
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<PowerAutomateResponse>();
            return Ok(new { OpenAIOutput = result.OpenAIOutput });
        }
        else
        {
            return StatusCode((int)response.StatusCode, "Failed to trigger Power Automate.");
        }
    }


    public class DispatchRequest
    {
        public string OrderId { get; set; }
        public string CustomerName { get; set; }
    }

    private class PowerAutomateResponse
    {
        public string OpenAIOutput { get; set; }
    }
    public class PowerAutomateForecastResponse
    {
        public double AzureMLDemandForecast { get; set; } 
        public string AzureOpenAISuggestions { get; set; }
    }
    [HttpGet("ReadinessCheck/{orderNumber}")]
    public async Task<IActionResult> GetOrderReadiness(string orderNumber)
    {
        try
        {
            using (var httpClient = new HttpClient())
            {
                var payload = new { OrderNumber = orderNumber };
                var requestContent = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync("https://prod-65.westus.logic.azure.com:443/workflows/bb01ffe4ef3c4038a2be139eb8b5624b/triggers/manual/paths/invoke?api-version=2016-06-01&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=Ttyk4Er2n2q0jigbdO7EhsAHrGruuqDI6a1xTgsxcjo", requestContent);
                response.EnsureSuccessStatusCode();
                var responseContent = await response.Content.ReadAsStringAsync();

                var readinessData = JsonConvert.DeserializeObject<ReadinessData>(responseContent);

                // Deserialize the MedicineDetails separately
                readinessData.MedicineDetails = JsonConvert.DeserializeObject<List<MedicineInfo>>(readinessData.MedicineDetailsString);

                return Ok(readinessData);
            }
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(500, $"Error contacting Power Automate: {ex.Message}");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }

    [HttpPost("fraud-check")]
    public async Task<ActionResult> CheckFraud([FromBody] FraudCheckRequest request)
    {
        try
        {
            // Create a new HttpClient to call Power Automate
            using var httpClient = new HttpClient();

            // Create a JSON payload with the order information
            var jsonPayload = JsonConvert.SerializeObject(new { OrderId = request.OrderNumber });
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            // Send a POST request to Power Automate
            var response = await httpClient.PostAsync("https://prod-173.westus.logic.azure.com:443/workflows/d74efa8a764b419699bcd6bc0645e4db/triggers/manual/paths/invoke?api-version=2016-06-01&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=0Yr9geIMh0a1oWIkjMEu5yTcrlXJaMLkrlFBlqU0Ci8", content);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();

                // Deserialize the response from Power Automate
                var fraudCheckResponse = JsonConvert.DeserializeObject<FraudCheckResponse>(result);

                if (fraudCheckResponse != null && !string.IsNullOrEmpty(fraudCheckResponse.FraudCheck))
                {
                    return Ok(new { FraudCheckResult = fraudCheckResponse.FraudCheck });
                }
            }
            else
            {
                return StatusCode((int)response.StatusCode, "Failed to trigger Power Automate.");
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred during fraud check: {ex.Message}");
        }

        return BadRequest("Failed to check for fraud.");
    }

    [HttpPost("forecast")]
    public async Task<ActionResult> ForecastDemand([FromBody] ForecastRequest request)
    {
        try
        {
            var payload = new
            {
                MedicineName = request.MedicineName,
                Manufacturer = request.Manufacturer,
                ForecastDate = request.ForecastDate,
                UnitPrice = request.UnitPrice,
                Discount = request.Discount
            };

            // Serialize the payload
            var jsonPayload = JsonConvert.SerializeObject(payload);

            using (var httpClient = new HttpClient())
            {
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                // Send the POST request to Power Automate
                var response = await httpClient.PostAsync("https://prod-49.westus.logic.azure.com:443/workflows/30d38e068f204a73a84533df44c7c563/triggers/manual/paths/invoke?api-version=2016-06-01&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=al3FHNygQrJ6rJFXTqr9PxLDf3BmEhaBlB6MxuYHbMo", content);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<List<PowerAutomateForecastResponse>>();
                    if (result != null && result.Count > 0)
                    {
                        return Ok(result);
                    }
                }
                else
                {
                    return StatusCode((int)response.StatusCode, "Failed to trigger Power Automate.");
                }
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }

        return BadRequest("Failed to forecast demand.");
    }


    [HttpGet]
    public async Task<IActionResult> GetOrders()
    {
        try
        {
            using (var httpClient = new HttpClient())
            {
                var dummyPayload = new { /* Add dummy data here */ };
                var content = new StringContent(JsonConvert.SerializeObject(dummyPayload), Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync("https://prod-162.westus.logic.azure.com:443/workflows/5bb8d235bbb54db48ea7fb390a201407/triggers/manual/paths/invoke?api-version=2016-06-01&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=PF9-AbnKtbedeWRb-OyPHdww4x5hSTNt10dLNwo3xqw", content);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                var orders = JsonConvert.DeserializeObject<List<Order>>(responseContent);

                return Ok(orders);
            }
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(500, $"Server error: {ex.Message}");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Server error: {ex.Message}");
        }
    }
}
public class ForecastRequest
{
    public string MedicineName { get; set; }
    public string Manufacturer { get; set; }
    public DateTime ForecastDate { get; set; }
    public string UnitPrice { get; set; }
    public string Discount { get; set; }
}
public class MedicineInfo
{
    [JsonProperty("AvailableStock")]
    public int QuantityInStock { get; set; }

    [JsonProperty("OrderQuantity")]
    public int OrderQuantity { get; set; }

    [JsonProperty("ProductName")]
    public string ProductName { get; set; }

    [JsonProperty("ImageUrl")]
    public string ImageUrl { get; set; }
}

public class ReadinessData
{
    public string AzureOpenAIOutput { get; set; }

    [JsonProperty("MedicineDetails")]
    public string MedicineDetailsString { get; set; }  // Change this to a string

    [JsonIgnore]
    public List<MedicineInfo> MedicineDetails { get; set; }  // This will be filled separately
}

public class Order
{
    public string OrderNumber { get; set; }
    public DateTime OrderDate { get; set; }
    public string CustomerName { get; set; }
    public int TotalItems { get; set; }
    public string DispatchStatus { get; set; }
    public string Readiness { get; set; }
}

public class FraudCheckRequest
{
    public string OrderNumber { get; set; }
}

public class FraudCheckResponse
{
    public string FraudCheck { get; set; }
}
