// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityModel;

namespace IdentityServer7.Services
{
    /// <summary>
    /// Default handle generation service
    /// </summary>
    /// <seealso cref="IdentityServer7.Services.IHandleGenerationService" />
    public class DefaultHandleGenerationService : IHandleGenerationService
    {
        /// <summary>
        /// Generates a handle.
        /// </summary>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        public Task<string> GenerateAsync(int length)
        {
            return Task.FromResult(CryptoRandom.CreateUniqueId(length, CryptoRandom.OutputFormat.Hex));
        }
    }
}