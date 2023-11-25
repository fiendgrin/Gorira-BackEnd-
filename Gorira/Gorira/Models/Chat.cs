namespace Gorira.Models
{
    public class Chat:BaseEntity
    {
        public string? User1Id { get; set; }
        public AppUser? User1 { get; set; }
        public string? User2Id { get; set; }
        public AppUser? User2 { get; set; }

        public IEnumerable<ChatLog>? ChatLogs { get; set; }
    }
}
