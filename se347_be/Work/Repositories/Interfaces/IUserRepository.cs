using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using se347_be.Database;
using se347_be.Work.Database.Entity;

namespace se347_be.Work.Repositories.Interfaces
{
    public interface IUserRepository
    {
        public Task<string> AddNewUserAsync(AppUser user);
        public Task RemoveByIdAsync(Guid id);
        public Task<AppUser?> GetUserWithGmailAndPasswordAsync(string email, string password);
    }
}