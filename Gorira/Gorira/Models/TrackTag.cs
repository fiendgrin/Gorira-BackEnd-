namespace Gorira.Models
{
    public class TrackTag:BaseEntity
    {
        public int? TrackId { get; set; }
        public Track? Track { get; set; }
        public int? TagId { get; set; }
        public Tag? Tag { get; set; }
    }
}
