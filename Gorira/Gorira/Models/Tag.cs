using System.ComponentModel.DataAnnotations;

namespace Gorira.Models
{
    public class Tag : BaseEntity
    {
        [StringLength(30)]
        public string Name { get; set; }
    }
}
