// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer7.Validation;

namespace IdentityServer7.ResponseHandling
{
    /// <summary>
    /// Interface the token response generator
    /// </summary>
    public interface ITokenResponseGenerator
    {
        /// <summary>
        /// Processes the response.
        /// </summary>
        /// <param name="validationResult">The validation result.</param>
        /// <returns></returns>
        Task<TokenResponse> ProcessAsync(TokenRequestValidationResult validationResult);
    }
}