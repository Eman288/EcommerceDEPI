using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace EcommerceDEPI.Services
{
    public class CardService
    {
        private readonly string _jsonFilePath;

        public CardService(string webRootPath)
        {
            _jsonFilePath = Path.Combine(webRootPath, "data", "cards.json");
        }

        public async Task<List<CardDetails>> GetCardsAsync()
        {
            if (!File.Exists(_jsonFilePath))
            {
                return new List<CardDetails>();
            }

            using var stream = File.OpenRead(_jsonFilePath);
            return await JsonSerializer.DeserializeAsync<List<CardDetails>>(stream) ?? new List<CardDetails>();
        }

        public async Task<bool> IsCardValidAsync(string cardNumber, string cvv, string expirationDate)
        {
            var cards = await GetCardsAsync();
            return cards.Any(c =>
                c.CardNumber == cardNumber &&
                c.Cvv == cvv &&
                c.ExpirationDate == expirationDate);
        }
    }

    public class CardDetails
    {
        public string CardNumber { get; set; }
        public string Cvv { get; set; }
        public string ExpirationDate { get; set; }
    }
}