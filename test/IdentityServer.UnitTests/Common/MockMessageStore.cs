// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer7.Models;
using IdentityServer7.Storage;

namespace IdentityServer.UnitTests.Common
{
    public class MockMessageStore<TModel> : IMessageStore<TModel>
    {
        public Dictionary<string, Message<TModel>> Messages { get; set; } = new Dictionary<string, Message<TModel>>();

        public Task<Message<TModel>> ReadAsync(string? id)
        {
            if (id != null && Messages.TryGetValue(id, out var val))
            {
                return Task.FromResult(val);
            }

            return Task.FromResult(new Message<TModel>(default(TModel)));
        }

        public Task<string> WriteAsync(Message<TModel> message)
        {
            var id = Guid.NewGuid().ToString();
            Messages[id] = message;
            return Task.FromResult(id);
        }
    }
}