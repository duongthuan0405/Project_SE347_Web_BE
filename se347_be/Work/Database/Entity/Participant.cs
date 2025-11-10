using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace se347_be.Work.Database.Entity
{
    [Table("Participant")]
    public class Participant
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required, ForeignKey("ParticipantList")]
        public Guid ListId { get; set; }

        [Required, Column(TypeName = "varchar(255)")]
        public string FullName { get; set; } = null!;

        [Column(TypeName = "varchar(50)")]
        public string? StudentId { get; set; }

        [Required, Column(TypeName = "varchar(255)")]
        public string Email { get; set; } = null!;

        // Navigation
        public ParticipantList? ParticipantList { get; set; }
    }
}
