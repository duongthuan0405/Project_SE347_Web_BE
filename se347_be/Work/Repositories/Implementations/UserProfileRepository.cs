using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    }
}