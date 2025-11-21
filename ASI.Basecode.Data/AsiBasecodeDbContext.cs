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

        public virtual DbSet<Feedback> Feedbacks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                // Users table: Id (identity), Email, Name, Password, Role, Status, CreatedTime, CreatedBy, UpdatedTime, UpdatedBy

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedTime).HasColumnType("datetime");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                // Password column in SQL is varchar(max); do not restrict length here
                entity.Property(e => e.Password)
                    .IsRequired()
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedTime).HasColumnType("datetime");

                // Optionally add unique constraints on Email or Name if DB has them
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

                // UserID and AgentID are integers matching SQL schema

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

            modelBuilder.Entity<Feedback>(entity =>
            {
                 entity.ToTable("Feedback");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TicketId).IsRequired();
                entity.Property(e => e.Rating).IsRequired();
                entity.Property(e => e.Comment).HasMaxLength(500);
                entity.Property(e => e.FeedbackDate).HasColumnType("datetime");
                entity.Property(e => e.Status).HasMaxLength(50).IsRequired();

                // Add foreign key relationship
                entity.HasOne(e => e.Ticket)
                    .WithMany(t => t.Feedback)
                    .HasForeignKey(e => e.TicketId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        
        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
