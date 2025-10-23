using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace se347_be.DTOs
{
    public class TestEntityDTO
    {
        string? id;
        string? name;
        public string? Id { get => id; set => id = value; }
        public string? Name { get => name; set => name = value; }

        public TestEntityDTO()
        {
            
        }
        public TestEntityDTO(string name)
        {
            Name = name;
            Id = "";
        }

        public TestEntityDTO(string id, string name)
        {
            Name = name;
            Id = id;
        }
    }
}