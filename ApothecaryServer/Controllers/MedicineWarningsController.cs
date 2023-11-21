using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
[Route("api/[controller]")]
[ApiController]

public class MedicineWarningsController : ControllerBase
{
    public class MedicineWarningRequest
    {
        public string MedicineName { get; set; }
        public string UserName { get; set; }
    }

    private class MedicineWarningResponse
    {
        public string MedicineWarning { get; set; }
    }

    [HttpPost("get-warning")]
    public async Task<IActionResult> GetMedicineWarning([FromBody] MedicineWarningRequest request)
    {
        var powerAutomateUrl = "https://prod-67.westus.logic.azure.com:443/workflows/af2e8be8d5b44587b21474bade610da1/triggers/manual/paths/invoke?api-version=2016-06-01&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=moBYZJeqZqC7duGwDzo-nsuo-PLe4iS3Mj4FTnL9K8I";

        try
        {
            using var httpClient = new HttpClient();

            var requestData = new
            {
                MedicineName = request.MedicineName,
                UserName = request.UserName
            };

            var content = new StringContent(JsonConvert.SerializeObject(requestData), Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync(powerAutomateUrl, content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var responseData = JsonConvert.DeserializeObject<MedicineWarningResponse>(responseContent);
                return Ok(responseData);
            }
            else
            {
                return StatusCode((int)response.StatusCode, "Failed to fetch the medicine warnings.");
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }
}
