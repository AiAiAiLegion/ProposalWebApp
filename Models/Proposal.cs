using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProposalWebApp.Models
{
    public enum ProposalStatus : short
    {
        Created = 0,
        Deleted = 1,
        Approved = 2
    }

    public class Proposal
    {
        public int Id { get; set; }

        public int Number { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        public ProposalStatus Status { get; set; } = ProposalStatus.Created;

        [StringLength(200)]
        public string? Author { get; set; }

        [StringLength(200)]
        public string? Department { get; set; }

        public string FullNumber => $"{CreatedAt:yy}/{Number:D4}";

        public string TextStatus => Status switch
        {
            ProposalStatus.Created => "Создана",
            ProposalStatus.Deleted => "Удалена",
            ProposalStatus.Approved => "Утверждена",
            _ => "Неизвестно"
        };

        public List<ProposalMaterial> Materials { get; set; } = new();
    }
}
