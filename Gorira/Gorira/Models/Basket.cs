namespace Gorira.Models
{
    public class Basket : BaseEntity
    {
        public bool IsUnlimited { get; set; }
        public int? TrackId { get; set; }
        public Track? Track { get; set; }
        public string? UserId { get; set; }
        public AppUser? User { get; set; }
    }
}
