using Gorira.Models;
using Gorira.Enums;

namespace Gorira.ViewModels.TrackVMs
{
    public class FilterVM
    {
        public IEnumerable<Genre> Genres { get; set; }
        public IEnumerable<Mood> Moods { get; set; }
        public List<int>? selectedGenres { get; set; }
        public List<int>? selectedMoods { get; set; }
        public List<Key>? selectedKeys { get; set; }
        public double? minPrice { get; set; }
        public double? maxPrice { get; set; }
        public double? minBpm { get; set; }
        public double? maxBpm { get; set; }
    }
}
