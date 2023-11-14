using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Gorira.Models
{
    public class BaseEntity
    {
        public int Id { get; set; }
        public bool IsDeleted { get; set; }
        [Column(TypeName = "date")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        [StringLength(100)]
        public string CreatedBy { get; set; } = "System";
        [Column(TypeName = "date")]
        public DateTime? DeletedAt { get; set; }
        [StringLength(100)]
        public string? DeletedBy { get; set; }
        [Column(TypeName = "date")]
        public DateTime? UpdatedAt { get; set; }
        [StringLength(100)]
        public string? UpdatedBy { get; set; }
    }
}
