using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace se347_be.Work.DTOs.Authen
{
    public class SignInRequestDTO
    {
        string email = "";
        string password = "";

        public string Email { get => email; set => email = value; }
        public string Password { get => password; set => password = value; }
    }
}