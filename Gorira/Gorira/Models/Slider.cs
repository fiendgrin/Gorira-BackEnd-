using Gorira.Attributes.ValidationAttributes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gorira.Models
{
    public class Slider : BaseEntity
    {

        [StringLength(255)]
        public string? Image { get; set; }
        [StringLength(500)]
        public string Text { get; set; }
        [StringLength(100)]
        public string BtnText { get; set; }
        [StringLength(255)]
        public string Link { get; set; }
        [NotMapped]
        [FileTypes("image/png", "image/jpeg")]
        [MaxFileSize(10)]
        public IFormFile? ImageFile { get; set; }
    }
}
