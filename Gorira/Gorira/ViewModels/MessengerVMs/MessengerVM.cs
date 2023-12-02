using Gorira.Models;
using Microsoft.Build.Execution;

namespace Gorira.ViewModels.MessengerVMs
{
    public class MessengerVM
    {
        public IEnumerable<Chat>? Chats { get; set; }
        public AppUser CurrentUser { get; set; }
        public int? ChatId { get; set; }

        public Chat? showingChat { get; set; }
        public AppUser? userName { get; set; }
        public AppUser? otherUserName { get; set; }

        public int? page { get; set; }
    }
}
