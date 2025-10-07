using Microsoft.EntityFrameworkCore;
using ProposalWebApp.Models;


namespace ProposalWebApp.Data;

public class ProposalDbContext : DbContext
{
    public ProposalDbContext(DbContextOptions<ProposalDbContext> options) : base(options) { }

    public DbSet<Proposal> Proposals { get; set; } = default!;
    public DbSet<ProposalMaterial> ProposalMaterials { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Связь Proposal -> ProposalMaterial
        modelBuilder.Entity<Proposal>()
            .HasMany(p => p.Materials)
            .WithOne(m => m.Proposal)
            .HasForeignKey(m => m.ProposalId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ограничения по длинам 
        modelBuilder.Entity<Proposal>(entity =>
        {
            entity.Property(p => p.Author).HasMaxLength(200);
            entity.Property(p => p.Department).HasMaxLength(200);
        });

        modelBuilder.Entity<ProposalMaterial>(entity =>
        {
            entity.Property(m => m.Name).HasMaxLength(500);
            entity.Property(m => m.Code).HasMaxLength(10);
            entity.Property(m => m.Comment).HasMaxLength(2000);
        });

        // Индекс для ускорения поиска заявок по номеру/дате
        modelBuilder.Entity<Proposal>()
            .HasIndex(p => new { p.Number, p.CreatedAt });

        // Индекс для ускорения поиска материалов в рамках заявки
        modelBuilder.Entity<ProposalMaterial>()
            .HasIndex(m => new { m.ProposalId, m.Name });
    }
}
