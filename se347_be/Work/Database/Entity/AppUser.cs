using Microsoft.EntityFrameworkCore;
using se347_be.Work.Database.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace se347_be.Work.Database.Entity
{
    [Table("User")]
    [Index(nameof(Email), IsUnique = true)]
    public class AppUser
    {
        [Key]
        public Guid Id { get; set; }

        [Required, Column(TypeName = "varchar(255)")]
        public string Email { get; set; } = null!;

        [Required, Column(TypeName = "varchar(255)")]
        public string PasswordHash { get; set; } = null!;

        // Navigation 
        public AppUserProfile? AppUserProfile { get; set; }
        public List<Quiz>? QuizzesCreated { get; set; }
        public List<QuizParticipation>? Participations { get; set; }
       
    }
}