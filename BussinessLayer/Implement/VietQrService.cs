using System.Text;
using System.Text.Json;
using BussinessLayer.Interface;
using DataAccess.Models;
using Microsoft.Extensions.Configuration;

namespace BussinessLayer.Implement
{
    public class VietQrService : IVietQrService
    {
        private readonly IConfiguration _config;
        private readonly HttpClient _httpClient;

        public VietQrService(IConfiguration config, IHttpClientFactory httpClientFactory)
        {
            _config = config;
            _httpClient = httpClientFactory.CreateClient();
        }

        public async Task<string> GenerateQrCodeBase64Async(int amount, string description)
        {
            using var httpClient = new HttpClient();

            VietQrRequest request = new VietQrRequest
            {
                accountNo = _config["VietQrConfig:AccountNo"],
                accountName = _config["VietQrConfig:AccountName"],
                acqId = _config["VietQrConfig:AcqId"],
                amount = amount,
                addInfo = description,
                template = "compact",
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            Console.WriteLine(content);

            var response = await _httpClient.PostAsync(
                "https://api.vietqr.io/v2/generate",
                content
            );
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(result);
            return doc.RootElement.GetProperty("data").GetProperty("qrDataURL").GetString();
        }
    }
}
