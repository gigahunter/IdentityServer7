// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using FluentAssertions;
using IdentityModel;
using IdentityServer.UnitTests.Common;
using IdentityServer7.Configuration;
using IdentityServer7.Models;
using IdentityServer7.Services;
using IdentityServer7.Storage.Models;
using IdentityServer7.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityServer.UnitTests.Services.Default
{
    public class DefaultTokenServiceTests
    {
        private DefaultTokenService _subject;

        private MockClaimsService _mockClaimsService = new MockClaimsService();
        private MockReferenceTokenStore _mockReferenceTokenStore = new MockReferenceTokenStore();
        private MockTokenCreationService _mockTokenCreationService = new MockTokenCreationService();
        private DefaultHttpContext _httpContext = new DefaultHttpContext();
        private MockSystemClock _mockSystemClock = new MockSystemClock();
        private MockKeyMaterialService _mockKeyMaterialService = new MockKeyMaterialService();
        private IdentityServerOptions _options = new IdentityServerOptions();

        public DefaultTokenServiceTests()
        {
            _options.IssuerUri = "https://test.identityserver.io";

            var svcs = new ServiceCollection();
            svcs.AddSingleton(_options);
            _httpContext.RequestServices = svcs.BuildServiceProvider();

            _subject = new DefaultTokenService(
                _mockClaimsService,
                _mockReferenceTokenStore,
                _mockTokenCreationService,
                new HttpContextAccessor { HttpContext = _httpContext },
                _mockSystemClock,
                _mockKeyMaterialService,
                _options,
                TestLogger.Create<DefaultTokenService>());
        }

        [Fact]
        public async Task CreateAccessTokenAsync_should_include_aud_for_each_ApiResource()
        {
            var request = new TokenCreationRequest
            {
                ValidatedResources = new ResourceValidationResult()
                {
                    Resources = new Resources()
                    {
                        ApiResources =
                        {
                            new ApiResource("api1"){ Scopes = { "scope1" } },
                            new ApiResource("api2"){ Scopes = { "scope2" } },
                            new ApiResource("api3"){ Scopes = { "scope3" } },
                        },
                    },
                    ParsedScopes =
                    {
                        new ParsedScopeValue("scope1"),
                        new ParsedScopeValue("scope2"),
                        new ParsedScopeValue("scope3"),
                    }
                },
                ValidatedRequest = new ValidatedRequest()
                {
                    Client = new Client { }
                }
            };

            var result = await _subject.CreateAccessTokenAsync(request);

            result.Audiences.Count.Should().Be(3);
            result.Audiences.Should().BeEquivalentTo(new[] { "api1", "api2", "api3" });
        }

        [Fact]
        public async Task CreateAccessTokenAsync_when_no_apiresources_should_not_include_any_aud()
        {
            var request = new TokenCreationRequest
            {
                ValidatedResources = new ResourceValidationResult()
                {
                    Resources = new Resources()
                    {
                        ApiScopes =
                        {
                            new ApiScope("scope1"),
                            new ApiScope("scope2"),
                            new ApiScope("scope3"),
                        },
                    },
                    ParsedScopes =
                    {
                        new ParsedScopeValue("scope1"),
                        new ParsedScopeValue("scope2"),
                        new ParsedScopeValue("scope3"),
                    }
                },
                ValidatedRequest = new ValidatedRequest()
                {
                    Client = new Client { }
                }
            };

            var result = await _subject.CreateAccessTokenAsync(request);

            result.Audiences.Count.Should().Be(0);
        }

        [Fact]
        public async Task CreateAccessTokenAsync_when_no_session_should_not_include_sid()
        {
            var request = new TokenCreationRequest
            {
                ValidatedResources = new ResourceValidationResult(),
                ValidatedRequest = new ValidatedRequest()
                {
                    Client = new Client { },
                    SessionId = null
                }
            };

            var result = await _subject.CreateAccessTokenAsync(request);

            result.Claims.SingleOrDefault(x => x.Type == JwtClaimTypes.SessionId).Should().BeNull();
        }

        [Fact]
        public async Task CreateAccessTokenAsync_when_session_should_include_sid()
        {
            var request = new TokenCreationRequest
            {
                ValidatedResources = new ResourceValidationResult(),
                ValidatedRequest = new ValidatedRequest()
                {
                    Client = new Client { },
                    SessionId = "123"
                }
            };

            var result = await _subject.CreateAccessTokenAsync(request);

            result.Claims.SingleOrDefault(x => x.Type == JwtClaimTypes.SessionId).Value.Should().Be("123");
        }
    }
}