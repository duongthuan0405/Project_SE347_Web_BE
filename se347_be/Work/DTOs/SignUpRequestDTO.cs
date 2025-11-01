using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace se347_be.Work.DTOs
{
    public class SignUpRequestDTO
    {
        string email = "";
        string password = "";
        string lastName = "";
        string firstName = "";

        public string Email { get => email; set => email = value; }
        public string Password { get => password; set => password = value; }
        public string LastName { get => lastName; set => lastName = value; }
        public string FirstName { get => firstName; set => firstName = value; }
    }
}