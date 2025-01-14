﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer7.Validation;
using System.Collections.Specialized;
using System.Security.Claims;

namespace IdentityServer.UnitTests.Endpoints.EndSession
{
    class StubEndSessionRequestValidator : IEndSessionRequestValidator
    {
        public EndSessionValidationResult EndSessionValidationResult { get; set; } = new EndSessionValidationResult();
        public EndSessionCallbackValidationResult EndSessionCallbackValidationResult { get; set; } = new EndSessionCallbackValidationResult();

        public Task<EndSessionValidationResult> ValidateAsync(NameValueCollection parameters, ClaimsPrincipal subject)
        {
            return Task.FromResult(EndSessionValidationResult);
        }

        public Task<EndSessionCallbackValidationResult> ValidateCallbackAsync(NameValueCollection parameters)
        {
            return Task.FromResult(EndSessionCallbackValidationResult);
        }
    }
}
