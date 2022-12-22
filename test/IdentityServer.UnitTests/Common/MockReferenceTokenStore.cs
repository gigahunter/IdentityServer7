using IdentityServer7.Storage.Models;
using IdentityServer7.Storage.Stores;

namespace IdentityServer.UnitTests.Common
{
    internal class MockReferenceTokenStore : IReferenceTokenStore
    {
        public Task<Token> GetReferenceTokenAsync(string handle)
        {
            throw new NotImplementedException();
        }

        public Task RemoveReferenceTokenAsync(string handle)
        {
            throw new NotImplementedException();
        }

        public Task RemoveReferenceTokensAsync(string subjectId, string clientId)
        {
            throw new NotImplementedException();
        }

        public Task<string> StoreReferenceTokenAsync(Token token)
        {
            throw new NotImplementedException();
        }
    }
}