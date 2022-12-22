using IdentityServer7.Services;
using IdentityServer7.Validation;
using System.Security.Claims;

namespace IdentityServer.UnitTests.Common
{
    class MockClaimsService : IClaimsService
    {
        public List<Claim> IdentityTokenClaims { get; set; } = new List<Claim>();
        public List<Claim> AccessTokenClaims { get; set; } = new List<Claim>();

        public Task<IEnumerable<Claim>> GetIdentityTokenClaimsAsync(ClaimsPrincipal subject, ResourceValidationResult resources, bool includeAllIdentityClaims, ValidatedRequest request)
        {
            return Task.FromResult(IdentityTokenClaims.AsEnumerable());
        }

        public Task<IEnumerable<Claim>> GetAccessTokenClaimsAsync(ClaimsPrincipal subject, ResourceValidationResult resources, ValidatedRequest request)
        {
            return Task.FromResult(AccessTokenClaims.AsEnumerable());
        }
    }
}
