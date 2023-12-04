using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ApothecaryShared;
using static System.Net.WebRequestMethods;
using System.Net.Http;
using static IoTService;

[ApiController]
[Route("api/[controller]")]
public class IoTController : ControllerBase
{
    private static readonly Dictionary<string, DeviceClient> deviceClients = new();
    private static readonly Dictionary<string, CancellationTokenSource> cancellationTokens = new();


    private readonly IoTService _iotService;

    public IoTController(IoTService iotService)
    {
        _iotService = iotService;
    }

    // POST api/iot/start
    [HttpPost("start")]
    public async Task<IActionResult> StartSimulation([FromBody] SimulationRequest request)
    {
        try
        {
            if (deviceClients.ContainsKey(request.DeviceId))
            {
                return BadRequest($"Simulation already running for {request.DeviceId}.");
            }

            if (!connectionStrings.TryGetValue(request.DeviceId, out var connectionString))
            {
                return NotFound($"Connection string not found for {request.DeviceId}.");
            }

            var deviceClient = DeviceClient.CreateFromConnectionString(connectionString, TransportType.Mqtt);
            deviceClients[request.DeviceId] = deviceClient;

            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokens[request.DeviceId] = cancellationTokenSource;
            var token = cancellationTokenSource.Token;

            _ = Task.Run(async () => await RunSimulationAsync(request, token));

            return Ok($"Started simulation for {request.DeviceId}");
        }
        catch (Exception ex)
        {
            // Log the exception or handle it as needed
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpPost("escalate")]
    public async Task<IActionResult> EscalateAnomaly([FromBody] EscalationData escalationData)
    {
        try
        {
            // Serialize the escalation data as JSON
            var jsonData = JsonConvert.SerializeObject(escalationData);

            // Define the Power Automate URL
            var powerAutomateUrl = "https://1prod-23.westus.logic.azure.com:443/workflows/fe44874f2f8e4f7d9a63e3342a0e8a2c/triggers/manual/paths/invoke?api-version=2016-06-01&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=poRYegX-HFRO_QjD7J-UVUnANZK75yLmx8YfttpKFao";

            // Create an HTTP request to send the data to Power Automate
            using (var httpClient = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Post, powerAutomateUrl)
                {
                    Content = new StringContent(jsonData, Encoding.UTF8, "application/json")
                };

                // Send the HTTP request
                var response = await httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    // Data sent to Power Automate successfully
                    return Ok("Anomaly escalated successfully.");
                }
                else
                {
                    // Handle the case where the request to Power Automate was not successful
                    return StatusCode((int)response.StatusCode, "Failed to escalate anomaly to Power Automate.");
                }
            }
        }
        catch (Exception ex)
        {
            // Log the exception or handle it as needed
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpGet("getlatestdata")]    
    public async Task<IActionResult> GetLatestData()
    {
        try
        {
            var httpClient = new HttpClient();
            var dummyPayload = new { /* Add dummy data here */ };
            var content = new StringContent(JsonConvert.SerializeObject(dummyPayload), Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync("https://1prod-97.westus.logic.azure.com:443/workflows/551fd3ca60c6430483b8a3b1bc424bc1/triggers/manual/paths/invoke?api-version=2016-06-01&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=BsIDp8Y2RuNZXaxD_s1ir4WJhN2ax3tRgyYbibQSup0", content);
            response.EnsureSuccessStatusCode();
                    

            if (response.IsSuccessStatusCode)
            {
                // Parse the response JSON
                var responseContent = await response.Content.ReadAsStringAsync();
                var responseData = JsonConvert.DeserializeObject<LatestDataResponse>(responseContent);

                return Ok(responseData);
            }
            else
            {
                // Handle the case where the request to Power Automate was not successful
                return StatusCode((int)response.StatusCode, "Failed to fetch latest data from Power Automate.");
            }
        }
        catch (Exception ex)
        {
            // Log the exception or handle it as needed
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    public class RealTimeEvent
    {
        public string Temperature { get; set; }
        public string Humidity { get; set; }
        public string DeviceName { get; set; }
        public string OccurrenceDate { get; set; }
        public string OpenAIPrediction { get; set; }
    }
    public class AnomalyData
    {
        public string Temperature { get; set; }
        public string Humidity { get; set; }
        public string DeviceName { get; set; }
        public string OccurenceTime { get; set; }
        public string OpenAIPrediction { get; set; }
    }
    public class LatestDataResponse
    {
        public string LatestAnomaly { get; set; }
        public string RealTimeEvents { get; set; }
    }
    private static readonly Dictionary<string, string> connectionStrings = new Dictionary<string, string>
{
    { "Refrigeration01", "HostName=dh-hack.azure-devices.net;DeviceId=Refrigeration01;SharedAccessKey=i3/NTwmGf9bW26lojR7VogalDgiWt+uttAIoTMb0LiI=" },
    { "Refrigeration02", "HostName=dh-hack.azure-devices.net;DeviceId=Refrigeration02;SharedAccessKey=vOE11VyYca3+njZQfUfJQCUa7M8lg9vHsAIoTH7X0hQ=" },
    { "Refrigeration03", "HostName=dh-hack.azure-devices.net;DeviceId=Refrigeration03;SharedAccessKey=8ynmSkDl/QZEcNB712uUygvMnC1dBHru2AIoTNjrPtw=" },
    { "Refrigeration04", "HostName=dh-hack.azure-devices.net;DeviceId=Refrigeration04;SharedAccessKey=kG/JcqmrYTTWug52uuLKN4t7Xcf0u8GOLAIoTHGmtE8=" }
};

    [HttpGet("updates")]
    public IActionResult GetUpdates()
    {
        var updates = _iotService.GetUpdates();
        return Ok(updates);
    }

    // POST api/iot/stop/{deviceId}
    [HttpPost("stop/{deviceId}")]
    public IActionResult StopSimulation(string deviceId)
    {
        if (!deviceClients.ContainsKey(deviceId))
        {
            return NotFound($"No simulation found for {deviceId}.");
        }

        cancellationTokens[deviceId].Cancel();
        deviceClients[deviceId].Dispose();
        deviceClients.Remove(deviceId);
        cancellationTokens.Remove(deviceId);

        return Ok($"Stopped simulation for {deviceId}");
    }

    private async Task RunSimulationAsync(SimulationRequest request, CancellationToken token)
    {
        var currentTemperature = request.CurrentTemperature;
        var currentHumidity = request.CurrentHumidity;

        while (!token.IsCancellationRequested)
        {
            if (request.IncrementTemperature)
            {
                currentTemperature = IncrementValue(currentTemperature, request.IncrementTemperatureValue, request.MinTemperature, request.MaxTemperature);
            }

            if (request.IncrementHumidity)
            {
                currentHumidity = IncrementValue(currentHumidity, request.IncrementHumidityValue, request.MinHumidity, request.MaxHumidity);
            }

            var messageString = CreateJSON(request.DeviceId, currentTemperature, currentHumidity);
            var message = new Message(Encoding.ASCII.GetBytes(messageString))
            {
                ContentType = "application/json",
                ContentEncoding = "UTF-8"
            };

            await deviceClients[request.DeviceId].SendEventAsync(message);
            await Task.Delay(request.MessageInterval * 1000, token);
        }
    }

    private double IncrementValue(double currentValue, double incrementValue, double minValue, double maxValue)
    {
        var newValue = currentValue + incrementValue;
        //if (newValue > maxValue) newValue = minValue;
        return newValue;
    }

    private string CreateJSON(string deviceId, double temperature, double humidity)
    {
        var data = new
        {
            DeviceId = deviceId,
            Temperature = temperature,
            Humidity = humidity
        };
        return JsonConvert.SerializeObject(data);
    }

   
}

 

public class IoTService
{
    private readonly List<string> _messages = new List<string>();

    // Other fields and methods related to the IoT simulation

    public void AddMessage(string message)
    {
        _messages.Add(message);
        // Optionally, keep only a certain number of recent messages
    }

    public IEnumerable<string> GetUpdates()
    {
        return _messages;
    }

    public class EscalationData
    {
        public string DeviceName { get; set; }
        public string Temperature { get; set; }
        public string Humidity { get; set; }
        public string OccurrenceTime { get; set; }
    }
}