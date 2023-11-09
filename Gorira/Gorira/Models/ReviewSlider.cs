using System.ComponentModel.DataAnnotations;

namespace Gorira.Models
{
    public class ReviewSlider:BaseEntity
    {
        [StringLength(255)]
        public string Image { get; set; }
        [StringLength(255)]
        public string BackgroundImage { get; set; }
        [StringLength(1000)]
        public string Text { get; set; }
    }
}
