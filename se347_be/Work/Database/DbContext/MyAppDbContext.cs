using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using se347_be.Database.DbEntity;
using se347_be.Work.Database.Entities;
using se347_be.Work.Database.Entity;

namespace se347_be.Database
{
    public class MyAppDbContext : DbContext
    {
        public MyAppDbContext(DbContextOptions<MyAppDbContext> options) : base(options) { }
        
        public DbSet<TestEntity> TestEntities { get; set; }
        public DbSet<AppUser> Users { get; set; }
        public DbSet<AppUserProfile> UserProfiles { get; set; }
        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<QuizParticipation> QuizParticipations { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Answer> Answers { get; set; }
        public DbSet<AnswerSelection> AnswerSelections { get; set; }
        public DbSet<PendingUser> PendingUsers { get; set; }
        public DbSet<ParticipantList> ParticipantLists { get; set; }
        public DbSet<Participant> Participants { get; set; }
        public DbSet<QuizSourceDocument> QuizSourceDocuments { get; set; }
        public DbSet<QuizQuestion> QuizQuestions { get; set; }
        public DbSet<QuizInvitation> QuizInvitations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // QuizQuestion: Many-to-many relationship (Quiz <-> Question)
            modelBuilder.Entity<QuizQuestion>()
                .HasKey(qq => new { qq.QuizId, qq.QuestionId });

            modelBuilder.Entity<QuizQuestion>()
                .HasOne(qq => qq.Quiz)
                .WithMany(q => q.QuizQuestions)
                .HasForeignKey(qq => qq.QuizId);

            modelBuilder.Entity<QuizQuestion>()
                .HasOne(qq => qq.Question)
                .WithMany(q => q.QuizQuestions)
                .HasForeignKey(qq => qq.QuestionId);

            // AnswerSelection: Allow multiple answers per question (multi-choice support)
            // But prevent duplicate (same answer selected twice)
            modelBuilder.Entity<AnswerSelection>()
                .HasIndex(a => new { a.ParticipationId, a.AnswerId })
                .IsUnique();

            // Unique constraint: One StudentId can only participate once per Quiz
            // This prevents cheating by using different emails with same StudentId
            // NOTE: Commenting out unique constraint as database has duplicate entries (MaxTimesCanAttempt > 1)
            // modelBuilder.Entity<QuizParticipation>()
            //     .HasIndex(p => new { p.QuizId, p.StudentId })
            //     .IsUnique()
            //     .HasFilter("\"StudentId\" IS NOT NULL"); // Only apply when StudentId is provided
        }

        
    }
}