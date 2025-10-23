using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace se347_be.Database.DbEntity
{
    [Table("TestEntity")]
    public class TestEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        [Column(TypeName = "VarChar(20)")]
        public string Name { get; set; }

        public TestEntity()
        {
            Name = "";
        }
        
        public TestEntity(string name)
        {
            Name = name;
        }

    }
}