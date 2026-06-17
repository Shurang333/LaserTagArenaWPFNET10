using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using LaserTagArenaWPFNET10.Models;

namespace LaserTagArenaWPFNET10.Services
{
    public class NeuralNetworkService
    {
        private static readonly HttpClient Http = new HttpClient();

        public List<Equipment> GetRecommendationsByTags(string tags, List<Equipment> catalog, int maxCount = 5)
        {
            if (string.IsNullOrWhiteSpace(tags) || catalog == null || !catalog.Any())
                return catalog?.Take(maxCount).ToList() ?? new List<Equipment>();

            var tagSet = tags.ToLower()
                .Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.Trim())
                .Where(t => t.Length > 0)
                .ToHashSet();

            return catalog
                .Select(e => new { Equipment = e, Score = CalculateTagScore(e, tagSet) })
                .Where(x => x.Score > 0)
                .OrderByDescending(x => x.Score)
                .ThenBy(x => x.Equipment.Name)
                .Take(maxCount)
                .Select(x => x.Equipment)
                .ToList();
        }

        public async Task<string> GetAiAdviceAsync(string userQuery, List<Equipment> catalog)
        {
            var config = App.Configuration;
            string apiKey = config?["NeuralNetwork:ApiKey"] ?? string.Empty;
            string apiUrl = config?["NeuralNetwork:ApiUrl"] ?? string.Empty;

            if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(apiUrl))
                return GetLocalAiAdvice(userQuery, catalog);

            try
            {
                string catalogText = string.Join("\\n", catalog.Take(20).Select(e =>
                    $"- {e.Name} ({e.CategoryName}), теги: {e.Tags}, цена: {e.CurrentPrice:N0}"));

                string model = config?["NeuralNetwork:Model"] ?? "gpt-3.5-turbo";
                string payload = $@"{{""model"":""{model}"",""messages"":[{{""role"":""system"",""content"":""Ты консультант магазина лазертаг-оборудования.""}},{{""role"":""user"",""content"":""Запрос: {EscapeJson(userQuery)}. Каталог: {EscapeJson(catalogText)}""}}],""max_tokens"":400}}";

                var request = new HttpRequestMessage(HttpMethod.Post, apiUrl);
                request.Headers.Add("Authorization", $"Bearer {apiKey}");
                request.Content = new StringContent(payload, Encoding.UTF8, "application/json");

                var response = await Http.SendAsync(request);
                string body = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    return GetLocalAiAdvice(userQuery, catalog) + $"\n\n(API: {response.StatusCode})";

                string content = ExtractJsonStringValue(body, "content");
                return string.IsNullOrWhiteSpace(content) ? GetLocalAiAdvice(userQuery, catalog) : content;
            }
            catch (Exception ex)
            {
                return GetLocalAiAdvice(userQuery, catalog) + $"\n\n(Ошибка API: {ex.Message})";
            }
        }

        public List<Equipment> GetRecommendationsForQuery(string userQuery, List<Equipment> catalog, int maxCount = 5)
        {
            if (catalog == null || !catalog.Any())
                return new List<Equipment>();

            string query = (userQuery ?? "").Trim().ToLower();
            if (string.IsNullOrEmpty(query))
                return catalog.Take(maxCount).ToList();

            var keywords = query
                .Split(new[] { ' ', ',', '.', ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(w => w.Length > 1)
                .ToHashSet();

            var recommended = catalog
                .Select(e => new
                {
                    Equipment = e,
                    Score = CalculateTagScore(e, keywords) +
                            CountWordMatches(e.Name, keywords) +
                            CountWordMatches(e.CategoryName, keywords) +
                            CountSubstringMatch(e.Name, query) +
                            CountSubstringMatch(e.Tags, query) +
                            CountSubstringMatch(e.CategoryName, query)
                })
                .Where(x => x.Score > 0)
                .OrderByDescending(x => x.Score)
                .ThenBy(x => x.Equipment.Name)
                .Take(maxCount)
                .Select(x => x.Equipment)
                .ToList();

            if (!recommended.Any())
                recommended = catalog
                    .Where(e => MatchesQuery(e, query))
                    .Take(maxCount)
                    .ToList();

            if (!recommended.Any())
                recommended = catalog.OrderBy(e => e.CurrentPrice).Take(Math.Min(3, maxCount)).ToList();

            return recommended;
        }

        public string GetLocalAiAdvice(string userQuery, List<Equipment> catalog)
        {
            var recommended = GetRecommendationsForQuery(userQuery, catalog, 5);

            var sb = new StringBuilder();
            sb.AppendLine("ИИ-рекомендация (локальный режим):");
            sb.AppendLine($"По запросу «{userQuery}» подходят:");
            foreach (var item in recommended)
                sb.AppendLine($"• {item.Name} — {item.CurrentPrice:N0} ₽ (теги: {item.Tags ?? "—"})");

            sb.AppendLine("\nДля облачной нейросети укажите NeuralNetwork:ApiKey в appsettings.json.");
            return sb.ToString();
        }

        private static int CountWordMatches(string? text, HashSet<string> keywords)
        {
            if (string.IsNullOrWhiteSpace(text)) return 0;
            return text.ToLower().Split(' ').Count(w => keywords.Contains(w));
        }

        private static int CountSubstringMatch(string? text, string query)
        {
            if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(query)) return 0;
            return text.ToLower().Contains(query) ? 2 : 0;
        }

        private static bool MatchesQuery(Equipment equipment, string query)
        {
            string haystack = $"{equipment.Name} {equipment.Tags} {equipment.CategoryName} {equipment.SKU}".ToLower();
            return haystack.Contains(query);
        }

        private static int CalculateTagScore(Equipment equipment, HashSet<string> tagSet)
        {
            if (string.IsNullOrWhiteSpace(equipment.Tags)) return 0;

            var itemTags = equipment.Tags.ToLower()
                .Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.Trim());

            return itemTags.Count(t => tagSet.Any(s => t.Contains(s) || s.Contains(t)));
        }

        private static string EscapeJson(string value) =>
            (value ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "").Replace("\n", "\\n");

        private static string? ExtractJsonStringValue(string json, string key)
        {
            string marker = $"\"{key}\":\"";
            int start = json.IndexOf(marker, StringComparison.Ordinal);
            if (start < 0) return null;
            start += marker.Length;

            var sb = new StringBuilder();
            for (int i = start; i < json.Length; i++)
            {
                if (json[i] == '\\' && i + 1 < json.Length)
                {
                    char next = json[i + 1];
                    if (next == 'n') sb.Append('\n');
                    else if (next == '"') sb.Append('"');
                    else sb.Append(next);
                    i++;
                    continue;
                }
                if (json[i] == '"') break;
                sb.Append(json[i]);
            }
            return sb.ToString();
        }
    }
}