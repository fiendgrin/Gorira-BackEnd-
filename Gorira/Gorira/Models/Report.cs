using System.ComponentModel.DataAnnotations.Schema;

namespace Gorira.Models
{
    public class Report:BaseEntity
    {
        // Reporter (the one who reports)
        public string? ReporterId { get; set; }
        [ForeignKey("ReporterId")]
        public AppUser? Reporter { get; set; }

        // Suspect (the one who gets reported)
        public string? SuspectId { get; set; }
        [ForeignKey("SuspectId")]
        public AppUser? Suspect { get; set; }
    }
}
