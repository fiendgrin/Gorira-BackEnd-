using System.ComponentModel.DataAnnotations;

namespace Gorira.ViewModels.AccountVMs
{
    public class EditSocialMediaVM
    {
        [StringLength(255)]
        public string? YouTube { get; set; }
        [StringLength(255)]
        public string? Instagram { get; set; }
        [StringLength(255)]
        public string? SoundCloud { get; set; }
        [StringLength(255)]
        public string? Twitter { get; set; }
        [StringLength(255)]
        public string? Facebook { get; set; }
        [StringLength(255)]
        public string? VK { get; set; }
    }
}
