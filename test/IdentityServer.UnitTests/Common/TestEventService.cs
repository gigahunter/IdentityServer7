// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using FluentAssertions;
using IdentityServer7.Events;
using IdentityServer7.Services;

namespace IdentityServer.UnitTests.Common
{
    public class TestEventService : IEventService
    {
        private Dictionary<Type, object> _events = new Dictionary<Type, object>();

        public Task RaiseAsync(Event evt)
        {
            _events.Add(evt.GetType(), evt);
            return Task.CompletedTask;
        }

        public T AssertEventWasRaised<T>()
            where T : class
        {
            _events.ContainsKey(typeof(T)).Should().BeTrue();
            return (T) _events.Where(x => x.Key == typeof(T)).Select(x => x.Value).First();
        }

        public bool CanRaiseEventType(EventTypes evtType)
        {
            return true;
        }
    }
}