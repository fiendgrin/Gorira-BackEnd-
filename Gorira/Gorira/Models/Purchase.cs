namespace Gorira.Models
{
    public class Purchase : BaseEntity
    {
        public string? UserId { get; set; }
        public AppUser? User { get; set; }
        public int? TrackId { get; set; }
        public Track? Track { get; set; }

        public double? Price { get; set; }
        public bool IsUnlimited { get; set; }
    }
}
