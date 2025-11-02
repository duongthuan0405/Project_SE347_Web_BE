using Microsoft.EntityFrameworkCore;
using se347_be.Database;
using se347_be.Work.Database.Entity;
using se347_be.Work.Repositories.Interfaces;

namespace se347_be.Work.Repositories.Implementations
{
    public class PendingUserRepository : IPendingUserRepository
    {
        MyAppDbContext _db;
        public PendingUserRepository(MyAppDbContext myAppDbContext)
        {
            _db = myAppDbContext;
        }

        public async Task<string> AddNewPendingUserAsync(PendingUser pendingUser)
        {
            PendingUser? oldPendingUser = await _db.PendingUsers.FirstOrDefaultAsync(p => p.Email == pendingUser.Email);
            if (oldPendingUser == null)
            {
                await _db.PendingUsers.AddAsync(pendingUser);
                await _db.SaveChangesAsync();
                return pendingUser.Email;
            }
            else
            {
                oldPendingUser.FirstName = pendingUser.FirstName;
                oldPendingUser.LastName = pendingUser.LastName;
                oldPendingUser.PasswordHash = pendingUser.PasswordHash;
                await _db.SaveChangesAsync();
                return oldPendingUser.Email;
            }
        }

        public async Task<string?> GenerateOTPForAsync(string to, bool isReGenerate)
        {
            PendingUser? pendingUser = await _db.PendingUsers.FirstOrDefaultAsync(p => p.Email == to);
            if (pendingUser != null)
            {
                bool isValidOTP = pendingUser.OTP != null && pendingUser.ExpireAt != null && DateTime.UtcNow <= pendingUser.ExpireAt;

                if (!isValidOTP || isReGenerate)
                {
                    pendingUser.ExpireAt = DateTime.UtcNow.AddMinutes(15);
                    pendingUser.OTP = new Random().Next(100000, 999999).ToString();
                    await _db.SaveChangesAsync();
                    return pendingUser.OTP;
                }
                return null;
            }
            throw new InvalidDataException("Email does not exist!");
        }

        public async Task RemovePendingUserAsync(string to)
        {
            var otp = await _db.PendingUsers.FirstOrDefaultAsync(otp => otp.Email == to);
            if (otp != null)
            {
                _db.PendingUsers.Remove(otp);
                await _db.SaveChangesAsync();
            }

        }

        public async Task<PendingUser> VerifyAccountAsync(string email, string otp)
        {
            PendingUser? pendingUser = await _db.PendingUsers.FirstOrDefaultAsync(p =>p.Email == email); 
            if (pendingUser == null)
            {
                throw new InvalidDataException("Email don't exist in Pending User List!");
            }
            else if(pendingUser.OTP == null || pendingUser.ExpireAt == null)
            {
                throw new InvalidDataException("This email didn't receive OTP!");
            }
            else 
            {
                if (DateTime.UtcNow > pendingUser.ExpireAt)
                {
                    throw new InvalidDataException("OTP was expired!");
                }
                else if (pendingUser.OTP != otp)
                {
                    throw new InvalidDataException("OTP is invalid!");
                }
                _db.PendingUsers.Remove(pendingUser);
                await _db.SaveChangesAsync();
                return pendingUser;
            }
        }
    }
}
