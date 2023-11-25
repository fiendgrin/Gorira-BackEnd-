using Gorira.DataAccessLayer;
using Gorira.Models;
using Gorira.ViewModels.ChatHubVMs;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Gorira.Hubs
{
    public class ChatHub : Hub
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IHttpContextAccessor _contextAccessor;
        public ChatHub(IHttpContextAccessor contextAccessor, AppDbContext context, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, IWebHostEnvironment env)
        {
            _contextAccessor = contextAccessor;
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }
        public async Task SendMessage(string user, string message,string chatId,string pfp)
        {
            int chatIdInt = int.Parse(chatId);

            if (_contextAccessor.HttpContext.User.Identity.IsAuthenticated && _contextAccessor.HttpContext.User.IsInRole("Member"))
            {
                Chat? chat = await _context.Chats.FirstOrDefaultAsync(c => c.Id == chatIdInt);

                if (chat != null)
                {
                    // Create a ChatLog entry and save the message
                    ChatLog chatLogEntry = new ChatLog
                    {
                        Message = message,
                        Messager = await _userManager.FindByIdAsync(user), // Current user sending the message
                        ChatId = chatIdInt
                    };

                    await _context.ChatLogs.AddAsync(chatLogEntry);
                    await _context.SaveChangesAsync();

                    // Send the message only to the other user involved in the chat
                    await Clients.User(user).SendAsync("ReceiveMessage", message, pfp);
                }
            }


        }



    }
}

#region tutorial
//private readonly IDictionary<string, UserRoomConnection> _connection;

//public ChatHub(IDictionary<string, UserRoomConnection> connection)
//{
//    _connection = connection;
//}

//public async Task JoinRoom(UserRoomConnection userRoomConnection,string? name) 
//{
//    await Groups.AddToGroupAsync(Context.ConnectionId, groupName: userRoomConnection.Room!);
//    _connection[Context.ConnectionId] = userRoomConnection;
//    await Clients.Group(userRoomConnection.Room!)
//        .SendAsync(method: "ReciveMessage", arg1: "Let's Program Bot", arg2: $"{userRoomConnection.User} Joined");
//}
//public async Task SendMessage(string message)
//{
//    if (_connection.TryGetValue(Context.ConnectionId, out UserRoomConnection userRoomConnection))
//    {
//        await Clients.Group(userRoomConnection.Room!)
//            .SendAsync("ReceiveMessage", userRoomConnection.User, message);
//    }

//}

//public Task SendConnectedUser(string room)
//{
//    var users = _connection.Values
//        .Where(x => x.Room == room)
//        .Select(x => x.User);
//    return Clients.Group(room).SendAsync("ConnectedUser", users);
//}
#endregion