using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;


[Route("api/[controller]")]
    [ApiController]
    public class MedicinesController : ControllerBase
    {
        [HttpGet("getall")]
        public async Task<IActionResult> GetAllMedicines()
    {
        //Add your Power Automate URL here,below URL is a private URL where you will not have access
        var powerAutomateUrl = "https://1prod-47.westus.logic.azure.com:443/workflows/1f133f4a7ea444a78bd791f00517c07b/triggers/manual/paths/invoke?api-version=2016-06-01&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=9EfPtq43yTAZB3cOmyaV1TBGaBOV3lEkeABZAbXKr-U"; // Replace with your actual Power Automate URL

        try
        {
            using var httpClient = new HttpClient();
            // Create an empty content for the POST request
            var content = new StringContent("", System.Text.Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(powerAutomateUrl, content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var medicineList = JsonConvert.DeserializeObject<List<string>>(responseContent);
                return Ok(medicineList);
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, $"Error fetching medicine list: {errorContent}");
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while fetching medicines: {ex.Message}");
        }
    }

    [HttpPost("getunitprice")]
    public async Task<IActionResult> GetUnitPrice([FromBody] MedicineRequestUnitPrice request)
    {
        //Add your Power Automate URL here,below URL is a private URL where you will not have access
        var powerAutomateUrl = "https://1prod-105.westus.logic.azure.com:443/workflows/f1d14728d08747d4951857368f188869/triggers/manual/paths/invoke?api-version=2016-06-01&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=pdvaGipJu3WGrOZi1AUki2blKeC930O_togbWz87eJ8"; // Replace with your actual Power Automate URL

        try
        {
            using var httpClient = new HttpClient();
            var jsonRequest = JsonConvert.SerializeObject(request);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(powerAutomateUrl, content);

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var responseData = JsonConvert.DeserializeObject<Dictionary<string, decimal>>(jsonResponse);

                return Ok(responseData);
            }
            else
            {
                // Handle the case where the Power Automate call is not successful
                return StatusCode((int)response.StatusCode, "Failed to retrieve unit price");
            }
        }
        catch (Exception ex)
        {
            // Handle exceptions
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
    public class MedicineRequestUnitPrice
    {
        public string MedicineName { get; set; }
    }
}

