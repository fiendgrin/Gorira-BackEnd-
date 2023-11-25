using System.ComponentModel.DataAnnotations;

namespace Gorira.Models
{
    public class ChatLog:BaseEntity
    {
        [StringLength(1000)]
        public string Message { get; set; }
        public AppUser Messager { get; set; }
        public int? ChatId { get; set; }
        public Chat? Chat { get; set; }

    }
}
