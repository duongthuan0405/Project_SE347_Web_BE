using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using se347_be.Database;
using se347_be.Work.Database.Entity;
using se347_be.Work.Repositories.Interfaces;

namespace se347_be.Work.Repositories.Implementations
{
    public class UserProfileRepository : IUserProfileRepository
    {
        MyAppDbContext _db;
        public UserProfileRepository(MyAppDbContext myAppDbContext)
        {
            _db = myAppDbContext;
        }
        public async Task AddNewUserProfileAsync(AppUserProfile appUserProfile)
        {
            await _db.UserProfiles.AddAsync(appUserProfile);
            Console.WriteLine(appUserProfile.FirstName);
            await _db.SaveChangesAsync();
        }

        public async Task<AppUserProfile?> GetProfileByIdAsync(Guid id)
        {
            return await _db.UserProfiles.FirstOrDefaultAsync(pf => pf.Id == id);
            throw new NotImplementedException();
        }

        public async Task<AppUserProfile?> UpdateUserProfileAsync(AppUserProfile appUserProfile)
        {
            var userProfile = await _db.UserProfiles.FirstOrDefaultAsync(pf => pf.Id == appUserProfile.Id);

            userProfile = appUserProfile;

            await _db.SaveChangesAsync();

            return userProfile;
            throw new NotImplementedException();

        }
    }
}