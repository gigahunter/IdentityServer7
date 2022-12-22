// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using FluentAssertions;
using IdentityServer.UnitTests.Common;
using IdentityServer7.Configuration;
using IdentityServer7.Hosting;
using Microsoft.AspNetCore.Http;
using static IdentityServer7.Constants;

namespace IdentityServer.UnitTests.Hosting
{
    public class EndpointRouterTests
    {
        private Dictionary<string, IdentityServer7.Hosting.Endpoint> _pathMap;
        private List<IdentityServer7.Hosting.Endpoint> _endpoints;
        private IdentityServerOptions _options;
        private EndpointRouter _subject;

        public EndpointRouterTests()
        {
            _pathMap = new Dictionary<string, IdentityServer7.Hosting.Endpoint>();
            _endpoints = new List<IdentityServer7.Hosting.Endpoint>();
            _options = new IdentityServerOptions();
            _subject = new EndpointRouter(_endpoints, _options, TestLogger.Create<EndpointRouter>());
        }

        [Fact]
        public void Endpoint_ctor_requires_path_to_start_with_slash()
        {
            Action a = () => new IdentityServer7.Hosting.Endpoint("ep1", "ep1", typeof(MyEndpointHandler));
            a.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Find_should_return_null_for_incorrect_path()
        {
            _endpoints.Add(new IdentityServer7.Hosting.Endpoint("ep1", "/ep1", typeof(MyEndpointHandler)));
            _endpoints.Add(new IdentityServer7.Hosting.Endpoint("ep2", "/ep2", typeof(MyOtherEndpointHandler)));

            var ctx = new DefaultHttpContext();
            ctx.Request.Path = new PathString("/wrong");
            ctx.RequestServices = new StubServiceProvider();

            var result = _subject.Find(ctx);
            result.Should().BeNull();
        }

        [Fact]
        public void Find_should_find_path()
        {
            _endpoints.Add(new IdentityServer7.Hosting.Endpoint("ep1", "/ep1", typeof(MyEndpointHandler)));
            _endpoints.Add(new IdentityServer7.Hosting.Endpoint("ep2", "/ep2", typeof(MyOtherEndpointHandler)));

            var ctx = new DefaultHttpContext();
            ctx.Request.Path = new PathString("/ep1");
            ctx.RequestServices = new StubServiceProvider();

            var result = _subject.Find(ctx);
            result.Should().BeOfType<MyEndpointHandler>();
        }

        [Fact]
        public void Find_should_not_find_nested_paths()
        {
            _endpoints.Add(new IdentityServer7.Hosting.Endpoint("ep1", "/ep1", typeof(MyEndpointHandler)));
            _endpoints.Add(new IdentityServer7.Hosting.Endpoint("ep2", "/ep2", typeof(MyOtherEndpointHandler)));

            var ctx = new DefaultHttpContext();
            ctx.Request.Path = new PathString("/ep1/subpath");
            ctx.RequestServices = new StubServiceProvider();

            var result = _subject.Find(ctx);
            result.Should().BeNull();
        }

        [Fact]
        public void Find_should_find_first_registered_mapping()
        {
            _endpoints.Add(new IdentityServer7.Hosting.Endpoint("ep1", "/ep1", typeof(MyEndpointHandler)));
            _endpoints.Add(new IdentityServer7.Hosting.Endpoint("ep1", "/ep1", typeof(MyOtherEndpointHandler)));

            var ctx = new DefaultHttpContext();
            ctx.Request.Path = new PathString("/ep1");
            ctx.RequestServices = new StubServiceProvider();

            var result = _subject.Find(ctx);
            result.Should().BeOfType<MyEndpointHandler>();
        }

        [Fact]
        public void Find_should_return_null_for_disabled_endpoint()
        {
            _endpoints.Add(new IdentityServer7.Hosting.Endpoint(EndpointNames.Authorize, "/ep1", typeof(MyEndpointHandler)));
            _endpoints.Add(new IdentityServer7.Hosting.Endpoint("ep2", "/ep2", typeof(MyOtherEndpointHandler)));

            _options.Endpoints.EnableAuthorizeEndpoint = false;

            var ctx = new DefaultHttpContext();
            ctx.Request.Path = new PathString("/ep1");
            ctx.RequestServices = new StubServiceProvider();

            var result = _subject.Find(ctx);
            result.Should().BeNull();
        }

        private class MyEndpointHandler : IEndpointHandler
        {
            public Task<IEndpointResult> ProcessAsync(HttpContext context)
            {
                throw new NotImplementedException();
            }
        }

        private class MyOtherEndpointHandler : IEndpointHandler
        {
            public Task<IEndpointResult> ProcessAsync(HttpContext context)
            {
                throw new NotImplementedException();
            }
        }

        private class StubServiceProvider : IServiceProvider
        {
            public object GetService(Type serviceType)
            {
                if (serviceType == typeof(MyEndpointHandler)) return new MyEndpointHandler();
                if (serviceType == typeof(MyOtherEndpointHandler)) return new MyOtherEndpointHandler();

                throw new InvalidOperationException();
            }
        }
    }
}