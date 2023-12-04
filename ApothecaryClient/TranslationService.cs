namespace ApothecaryClient
{
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using System.Net.Http.Json;
    using Newtonsoft.Json;
    using System.Text;

    public class TranslationService
    {
        private readonly HttpClient _httpClient;
        private readonly string _powerAutomateUrl;

        public TranslationService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            // Replace with your actual Power Automate HTTP trigger URL
            _powerAutomateUrl = "https://1prod-123.westus.logic.azure.com:443/workflows/1138ba05d8c8499a86d438b964ce0aef/triggers/manual/paths/invoke?api-version=2016-06-01&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=qRvmte9KBFk6gYWpzH_RUMq528XJoU8oLENbAYjVsmM";
        }

        public async Task<Dictionary<string, string>> GetTranslationsAsync(string locale, Dictionary<string, string> keysToTranslate)
        {
            try
            {
                var requestBody = new { locale, texts = keysToTranslate };
                var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(_powerAutomateUrl, content);

                if (!response.IsSuccessStatusCode)
                {
                    // Optionally, you can log the response content for debugging purposes
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Error calling Power Automate for translations: {errorResponse}");
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonResponse);
            }
            catch (Exception ex)
            {
                // Handle or log the exception as necessary
                Console.WriteLine($"Translation service encountered an exception: {ex.Message}");
                return new Dictionary<string, string>(); // Return an empty dictionary to avoid crashing the application
            }
        }
    }


}
