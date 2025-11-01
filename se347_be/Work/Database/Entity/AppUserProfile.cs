using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace se347_be.Work.Database.Entity
{
    [Table("UserProfile")]
    public class AppUserProfile
    {
        [Key, ForeignKey("AppUser")]
        public Guid Id { get; set; }

        [Required, Column(TypeName = "varchar(255)")]
        public string LastName { get; set; } = null!;

        [Required, Column(TypeName = "varchar(255)")]
        public string FirstName { get; set; } = null!;

        [Column(TypeName = "varchar(255)")]
        public string? Avatar { get; set; }

        // Navigation
        public AppUser AppUser { get; set; } = null!;
    }
}