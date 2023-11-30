using Gorira.Attributes.ValidationAttributes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gorira.Models
{
    public class ReviewSlider:BaseEntity
    {
        [StringLength(255)]
        public string? Image { get; set; }
        [StringLength(255)]
        public string? BackgroundImage { get; set; }
        [StringLength(1000)]
        public string Text { get; set; }

        [NotMapped]
        [FileTypes("image/png", "image/jpeg")]
        [MaxFileSize(10)]
        public IFormFile? ImageFile { get; set; }

        [NotMapped]
        [FileTypes("image/png", "image/jpeg")]
        [MaxFileSize(10)]
        public IFormFile? BackgroundImageFile { get; set; }
    }
}
