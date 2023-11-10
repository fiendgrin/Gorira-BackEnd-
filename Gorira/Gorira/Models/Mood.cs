using System.ComponentModel.DataAnnotations;

namespace Gorira.Models
{
    public class Mood:BaseEntity
    {
        [StringLength(255)]
        public string Name { get; set; }
    }
}
