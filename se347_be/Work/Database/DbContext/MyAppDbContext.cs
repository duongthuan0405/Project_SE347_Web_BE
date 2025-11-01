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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AnswerSelection>().HasKey(ansSel => new { ansSel.AnswerId, ansSel.ParticipationId });
        }

        
    }
}