using ASI.Basecode.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace ASI.Basecode.Data
{
    public partial class AsiBasecodeDBContext : DbContext
    {
        public AsiBasecodeDBContext()
        {
        }

        public AsiBasecodeDBContext(DbContextOptions<AsiBasecodeDBContext> options)
            : base(options)
        {
        }

        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Article> Articles { get; set; }
        public virtual DbSet<Ticket> Tickets { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(e => e.UserId, "UQ__Users__1788CC4D5F4A160F")
                    .IsUnique();

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedTime).HasColumnType("datetime");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedTime).HasColumnType("datetime");

                entity.Property(e => e.UserId)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Article>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).HasMaxLength(150).IsRequired();
                entity.Property(e => e.Category).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Body).IsRequired();
            });

            modelBuilder.Entity<Ticket>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Summary).HasMaxLength(50).IsRequired();

                // UserID and AgentID are integers (no string length configuration)

                entity.Property(e => e.Status).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Type).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Description).IsRequired();
                entity.Property(e => e.Priority).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Category).HasMaxLength(50).IsRequired();

                entity.Property(e => e.ResolvedAt).HasColumnType("datetime");
                entity.Property(e => e.DueDate).HasColumnType("datetime");
                entity.Property(e => e.CreatedTime).HasColumnType("datetime");
                entity.Property(e => e.UpdatedTime).HasColumnType("datetime");

                entity.Property(e => e.CreatedBy)
                      .HasMaxLength(50)
                      .IsUnicode(false);

                entity.Property(e => e.UpdatedBy)
                      .HasMaxLength(50)
                      .IsUnicode(false);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        
        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
