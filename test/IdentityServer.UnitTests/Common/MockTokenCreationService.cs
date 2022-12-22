using IdentityServer7.Services;
using IdentityServer7.Storage.Models;

namespace IdentityServer.UnitTests.Common
{
    internal class MockTokenCreationService : ITokenCreationService
    {
        public string Token { get; set; } = Guid.NewGuid().ToString();

        public Task<string> CreateTokenAsync(Token token)
        {
            return Task.FromResult(Token);
        }
    }
}