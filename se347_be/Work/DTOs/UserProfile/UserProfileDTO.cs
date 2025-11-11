using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace se347_be.Work.DTOs.UserProfile
{
    public class UserProfileDTO
    {
        public string Id { get; set; } = "";
        public string? LastName { get; set; } = null!;
        public string? FirstName { get; set; } = null!;
        public string? Avatar { get; set; }

    }
}