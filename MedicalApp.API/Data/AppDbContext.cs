using MedicalApp.API.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MedicalApp.API.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // ===== DbSets =====
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<Resource> Resources { get; set; }
        public DbSet<Faq> Faqs { get; set; }
        public DbSet<SupportTicket> SupportTickets { get; set; }
        public DbSet<PodcastEpisode> PodcastEpisodes { get; set; }
        public DbSet<DoctorSession> DoctorSessions { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<PatientRecord> PatientRecords { get; set; }
        public DbSet<MoodEntry> MoodEntries { get; set; }
        public DbSet<AssessmentResult> AssessmentResults { get; set; }
        public DbSet<BotMessage> BotMessages { get; set; }

        // ===== New Tables =====
        public DbSet<ChatSession> ChatSessions { get; set; }
        public DbSet<FavoriteChat> FavoriteChats { get; set; }
        public DbSet<SavedItem> SavedItems { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ===================================================
            // 1. Message — Indexes for performance (no FK constraints — existing data)
            // ===================================================
            builder.Entity<Message>(entity =>
            {
                entity.HasIndex(m => m.ConversationId)
                      .HasDatabaseName("IX_Messages_ConversationId");

                entity.HasIndex(m => m.SenderId)
                      .HasDatabaseName("IX_Messages_SenderId");

                entity.HasIndex(m => m.ReceiverId)
                      .HasDatabaseName("IX_Messages_ReceiverId");

                entity.HasIndex(m => new { m.ConversationId, m.SentAt })
                      .HasDatabaseName("IX_Messages_ConversationId_SentAt");
            });

            // ===================================================
            // 2. BotMessage — Indexes for performance
            // ===================================================
            builder.Entity<BotMessage>(entity =>
            {
                entity.HasIndex(b => b.PatientId)
                      .HasDatabaseName("IX_BotMessages_PatientId");

                entity.HasIndex(b => b.SessionId)
                      .HasDatabaseName("IX_BotMessages_SessionId");

                entity.HasIndex(b => new { b.SessionId, b.SentAt })
                      .HasDatabaseName("IX_BotMessages_SessionId_SentAt");

                entity.HasOne(b => b.Patient)
                      .WithMany()
                      .HasForeignKey(b => b.PatientId)
                      .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(b => b.Session)
                      .WithMany(s => s.Messages)
                      .HasForeignKey(b => b.SessionId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ===================================================
            // 3. ChatSession — Indexes
            // ===================================================
            builder.Entity<ChatSession>(entity =>
            {
                entity.HasIndex(s => s.UserId)
                      .HasDatabaseName("IX_ChatSessions_UserId");

                entity.HasOne(s => s.User)
                      .WithMany()
                      .HasForeignKey(s => s.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ===================================================
            // 4. FavoriteChat — Unique constraint + Index
            // ===================================================
            builder.Entity<FavoriteChat>(entity =>
            {
                entity.HasIndex(f => new { f.UserId, f.ConversationId })
                      .IsUnique()
                      .HasDatabaseName("IX_FavoriteChats_UserId_ConversationId");

                entity.HasOne(f => f.User)
                      .WithMany()
                      .HasForeignKey(f => f.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ===================================================
            // 5. Patient Relations
            // ===================================================
            builder.Entity<Patient>()
                .HasOne(p => p.User)
                .WithOne()
                .HasForeignKey<Patient>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<DoctorSession>()
                .HasOne(ds => ds.Patient)
                .WithMany(p => p.Sessions)
                .HasForeignKey(ds => ds.PatientId)
                .HasPrincipalKey(p => p.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<PatientRecord>()
                .HasOne(pr => pr.Patient)
                .WithMany(p => p.Records)
                .HasForeignKey(pr => pr.PatientId)
                .HasPrincipalKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // ===================================================
            // 6. Post Relations
            // ===================================================
            builder.Entity<Comment>()
                .HasOne(c => c.Post)
                .WithMany(p => p.Comments)
                .HasForeignKey(c => c.PostId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Like>()
                .HasOne(l => l.Post)
                .WithMany(p => p.Likes)
                .HasForeignKey(l => l.PostId)
                .OnDelete(DeleteBehavior.NoAction);

            // ===================================================
            // 7. Decimal Columns
            // ===================================================
            builder.Entity<DoctorSession>()
                .Property(s => s.Price)
                .HasColumnType("decimal(18,2)");

            builder.Entity<Doctor>()
                .Property(d => d.SessionPrice)
                .HasColumnType("decimal(18,2)");
        }
    }
}