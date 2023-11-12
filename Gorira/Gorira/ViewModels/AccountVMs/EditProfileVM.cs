using Gorira.Attributes.ValidationAttributes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gorira.ViewModels.AccountVMs
{
    public class EditProfileVM
    {
       
        [StringLength(50)]
        public string? DisplayName { get; set; }
        [StringLength(255)]
        public string? FirstName { get; set; }
        [StringLength(255)]
        public string? LastName { get; set; }
        [StringLength(50)]
        public string? Location { get; set; }
        [StringLength(1000)]
        public string? AboutMe { get; set; }
        [StringLength(255)]
        public string? ProfilePicture { get; set; }
      

        [FileTypes("image/png", "image/jpeg")]
        [MaxFileSize(10)]
        [NotMapped]
        public IFormFile? ProfilePictureFile { get; set; }
    }
}
