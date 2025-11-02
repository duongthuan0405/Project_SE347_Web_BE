using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace se347_be.Work.Database.Entity
{
    [Table("PendingUser")]
    public class PendingUser
    {
        [Key]
        [Required, Column(TypeName = "varchar(255)")]
        public string Email { get; set; } = "";

        [Required, Column(TypeName = "varchar(255)")]
        public string PasswordHash { get; set; } = null!;

        [Required, Column(TypeName = "varchar(255)")]
        public string LastName { get; set; } = null!;

        [Required, Column(TypeName = "varchar(255)")]
        public string FirstName { get; set; } = null!;

        [Column(TypeName = "Char(6)")]
        public string? OTP { get; set; } = null; 

        [Column(TypeName = "Timestamptz")]
        public DateTime? ExpireAt { get; set; } = null;

    }
}
