using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using se347_be.Work.Database.Entity;

namespace se347_be.Work.Repositories.Interfaces
{
    public interface IUserProfileRepository
    {
        public Task AddNewUserProfileAsync(AppUserProfile appUserProfile);
        public Task<AppUserProfile?> UpdateUserProfileAsync(AppUserProfile appUserProfile);
        public Task<AppUserProfile?> GetProfileByIdAsync(Guid id);
    }
}