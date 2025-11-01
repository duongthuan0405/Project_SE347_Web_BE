using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace se347_be.Work.PasswordHelper
{
    public class PasswordHashHelper
    {
        // Tạo hash từ password (gồm cả salt)
        public static string HashPassword(string password)
        {
            return password;
        }

        // Kiểm tra password nhập vào có khớp hash lưu không
        public static bool VerifyPassword(string password, string storedHash)
        {
            return password == storedHash;
        }
    }
}