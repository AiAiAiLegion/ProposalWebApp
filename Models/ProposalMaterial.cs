using System.ComponentModel.DataAnnotations;

namespace ProposalWebApp.Models
{
    public enum MaterialStatus : short
    {
        Created = 0,
        Deleted = 1
    }

    public class ProposalMaterial
    {
        public int Id { get; set; }

        public MaterialStatus Status { get; set; } = MaterialStatus.Created;

        [Required]
        [StringLength(500)]
        public string? Name { get; set; }

        [StringLength(10)]
        [MaxLength(10)]
        public string? Code { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; } = 1;

        [StringLength(2000)]
        public string? Comment { get; set; }

        public int ProposalId { get; set; }
        public Proposal Proposal { get; set; } = default!;

        public string TextStatus => Status switch
        {
            MaterialStatus.Created => "Создан",
            MaterialStatus.Deleted => "Удалён",
            _ => "Неизвестно"
        };
    }
}
