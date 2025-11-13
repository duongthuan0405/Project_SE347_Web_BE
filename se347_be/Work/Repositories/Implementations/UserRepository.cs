using Microsoft.EntityFrameworkCore;
using Npgsql;
using se347_be.Database;
using se347_be.Work.Database.Entity;
using se347_be.Work.PasswordHelper;
using se347_be.Work.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace se347_be.Work.Repositories.Implementations
{
    public class UserRepository : IUserRepository
    {
        MyAppDbContext _db;
        public UserRepository(MyAppDbContext myAppDbContext)
        {
            _db = myAppDbContext;
        }

        public async Task<Guid> AddNewUserAsync(AppUser user)
        {
            try
            {
                await _db.Users.AddAsync(user);
                await _db.SaveChangesAsync();
                return user.Id;
            }
            catch (Exception ex)
            {
                if (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
                {
                    if (pgEx.ConstraintName == "IX_User_Email")
                        throw new InvalidDataException("Email has been used!");

                    throw new InvalidDataException("Unique Constrain Exception");
                }

                throw;
            }
        }

        public Task<AppUser?> GetUserByIdAsync(Guid id)
        {
            var user = _db.Users.FirstOrDefaultAsync(u => u.Id == id);
            return user;
        }

        public async Task<AppUser?> GetUserWithGmailAndPasswordAsync(string email, string password)
        {
            AppUser? user = await _db.Users.FirstOrDefaultAsync(us => us.Email == email);
            if(user == null)
            {
                return null;
            }
            if (!PasswordHashHelper.VerifyPassword(password, user.PasswordHash))
            {
                return null;
            }
            return user;
        }

        public async Task RemoveByIdAsync(Guid id)
        {
            var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == id);
            if (user != null)
            {
                _db.Users.Remove(user);
                await _db.SaveChangesAsync();
            }
        }
    }
}