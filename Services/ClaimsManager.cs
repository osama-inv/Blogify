using Blogify.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Blogify.Services
{
    public class ClaimsManager
    {
        private readonly ApplicationDbContext _DB_Contect;
        public ClaimsManager(ApplicationDbContext DB_Contect)
        {
            _DB_Contect = DB_Contect;
        }
        public async Task<bool> HasClaimType(string userid, string claimtype)
        {
            return await _DB_Contect.UserClaims.AnyAsync(x => x.UserId == userid && x.ClaimType == claimtype);
        }
        public async Task<bool> ChnageClaimValue(string userid, Claim claim)
        {
            int affectedRows = await _DB_Contect.UserClaims
           .Where(c => c.UserId == userid && c.ClaimType == claim.Type)
           .ExecuteUpdateAsync(c => c.SetProperty(u => u.ClaimValue, claim.Value));

            return affectedRows > 0;
        }
        public async Task<string?> GetClaimValue(string userid, string claimType)
        {
            var TheclaimType = await _DB_Contect.UserClaims
           .Where(c => c.UserId == userid && c.ClaimType == claimType).Select(c => c.ClaimValue).FirstOrDefaultAsync();

            return TheclaimType;
        }
        static public Claim? GetLanguageClaim(string claim)
        {
            var RequiredClaim = claim switch
            {
                "French" => new Claim("Language", "French"),
                "English" => new Claim("Language", "English"),
                _ => null
            };

            return RequiredClaim;
        }

    }
}
