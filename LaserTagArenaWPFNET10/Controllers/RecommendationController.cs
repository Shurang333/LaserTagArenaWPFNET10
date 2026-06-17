using LaserTagArenaWPFNET10.Models;
using LaserTagArenaWPFNET10.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LaserTagArenaWPFNET10.Controllers
{
    public class RecommendationController
    {
        private readonly NeuralNetworkService _neuralNetwork;
        private readonly EquipmentController _equipment;

        public RecommendationController(NeuralNetworkService neuralNetwork, EquipmentController equipment)
        {
            _neuralNetwork = neuralNetwork;
            _equipment = equipment;
        }

        public List<Equipment> GetByTags(string tags, int maxCount = 5)
        {
            var catalog = _equipment.GetAll();
            var byTags = _neuralNetwork.GetRecommendationsByTags(tags, catalog, maxCount);
            if (byTags.Any())
                return byTags;
            return _neuralNetwork.GetRecommendationsForQuery(tags, catalog, maxCount);
        }

        public List<Equipment> GetForQuery(string query, int maxCount = 5)
        {
            return _neuralNetwork.GetRecommendationsForQuery(query, _equipment.GetAll(), maxCount);
        }

        public List<Equipment> GetForEquipment(Equipment? source, int maxCount = 5)
        {
            string tags = source?.Tags ?? string.Empty;
            if (string.IsNullOrWhiteSpace(tags))
                tags = source?.CategoryName ?? source?.Name ?? string.Empty;
            return GetByTags(tags, maxCount);
        }

        public Task<string> GetAiAdviceAsync(string query) =>
            _neuralNetwork.GetAiAdviceAsync(query, _equipment.GetAll());
    }
}