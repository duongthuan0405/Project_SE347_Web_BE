using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace se347_be.Work.Database.Entity
{
    [Table("ParticipantList")]
    public class ParticipantList
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required, Column(TypeName = "varchar(255)")]
        public string Name { get; set; } = null!;

        [Required, ForeignKey("CreatorUser")]
        public Guid CreatorId { get; set; }

        // Navigation
        public AppUser? CreatorUser { get; set; }
        public ICollection<Participant>? Participants { get; set; }
    }
}
