using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using se347_be.Work.Database.Entities;

namespace se347_be.Work.Database.Entity
{
    [Table("User")]
    public class User
    {
        [Key]
        public Guid Id { get; set; }

        [Required, Column(TypeName = "varchar(255)")]
        public string Email { get; set; } = null!;

        [Required, Column(TypeName = "varchar(255)")]
        public string PasswordHash { get; set; } = null!;

        // Navigation 
        public UserProfile? UserProfile { get; set; }
        public List<Quiz>? QuizzesCreated { get; set; }
        public List<QuizParticipation>? Participations { get; set; }
    }
}