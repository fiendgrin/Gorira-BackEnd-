using Gorira.Models;

namespace Gorira.ViewModels.MessengerVMs
{
    public class MessengerVM
    {
        public IEnumerable<Chat>? Chats { get; set; }
        public AppUser CurrentUser { get; set; }
        public int? ChatId { get; set; }
    }
}
