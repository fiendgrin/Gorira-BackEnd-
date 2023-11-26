using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gorira.Models
{
    public class Mood:BaseEntity
    {
        [StringLength(255)]
        public string Name { get; set; }


        [InverseProperty("PrimaryMood")]
        public IEnumerable<Track>? PrimaryMoodTracks { get; set; }
        [InverseProperty("SecondaryMood")]
        public IEnumerable<Track>? SecondaryMoodTracks { get; set; }
    }
}
