﻿using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gorira.Models
{
    public class AppUser : IdentityUser
    {
        public bool IsActive { get; set; }
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


        [NotMapped]
        public IFormFile? ProfilePictureFile { get; set; }
    }
}
