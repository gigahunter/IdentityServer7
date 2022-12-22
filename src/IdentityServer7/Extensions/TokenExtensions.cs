// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityModel;
using IdentityServer7.Configuration;
using IdentityServer7.Storage.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Nodes;

//using Newtonsoft.Json.Linq;

namespace IdentityServer7.Extensions
{
    /// <summary>
    /// Extensions for Token
    /// </summary>
    public static class TokenExtensions
    {
        /// <summary>
        /// Creates the default JWT payload.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="clock">The clock.</param>
        /// <param name="options">The options</param>
        /// <param name="logger">The logger.</param>
        /// <returns></returns>
        /// <exception cref="Exception">
        /// </exception>
        public static JwtPayload CreateJwtPayload(this Token token, ISystemClock clock, IdentityServerOptions options, ILogger logger)
        {
            var payload = new JwtPayload(
                token.Issuer,
                null,
                null,
                clock.UtcNow.UtcDateTime,
                clock.UtcNow.UtcDateTime.AddSeconds(token.Lifetime));

            foreach (var aud in token.Audiences)
            {
                payload.AddClaim(new Claim(JwtClaimTypes.Audience, aud));
            }
            //token.Claims.Add(new Claim(JwtClaimTypes.Confirmation, "{\"asdf\":\"123\"}", valueType: "json"));
            //token.Claims.Add(new Claim(JwtClaimTypes.Confirmation, "{\"name\":\"666\"}", valueType: "json"));
            //token.Claims.Add(new Claim("Asd", "{\"name\":\"888\"}", valueType: "json"));
            //token.Claims.Add(new Claim("666", "[{\"Name\":\"666\"},{\"Name\":\"888\"}]", valueType: "json"));
            var amrClaims = token.Claims.Where(x => x.Type == JwtClaimTypes.AuthenticationMethod).ToArray();
            var scopeClaims = token.Claims.Where(x => x.Type == JwtClaimTypes.Scope).ToArray();
            var jsonClaims = token.Claims.Where(x => x.ValueType == IdentityServerConstants.ClaimValueTypes.Json).ToList();

            // add confirmation claim if present (it's JSON valued)
            if (token.Confirmation.IsPresent())
            {
                jsonClaims.Add(new Claim(JwtClaimTypes.Confirmation, token.Confirmation, IdentityServerConstants.ClaimValueTypes.Json));
            }

            var normalClaims = token.Claims
                .Except(amrClaims)
                .Except(jsonClaims)
                .Except(scopeClaims);

            payload.AddClaims(normalClaims);

            // scope claims
            if (!scopeClaims.IsNullOrEmpty())
            {
                var scopeValues = scopeClaims.Select(x => x.Value).ToArray();

                if (options.EmitScopesAsSpaceDelimitedStringInJwt)
                {
                    payload.Add(JwtClaimTypes.Scope, string.Join(" ", scopeValues));
                }
                else
                {
                    payload.Add(JwtClaimTypes.Scope, scopeValues);
                }
            }

            // amr claims
            if (!amrClaims.IsNullOrEmpty())
            {
                var amrValues = amrClaims.Select(x => x.Value).Distinct().ToArray();
                payload.Add(JwtClaimTypes.AuthenticationMethod, amrValues);
            }

            // deal with json types
            // calling ToArray() to trigger JSON parsing once and so later
            // collection identity comparisons work for the anonymous type
            try
            {
                var jsonTokens2 = jsonClaims
                    .Select(x => new { x.Type, JsonValue = JsonNode.Parse(x.Value) }).ToArray();

                var jsonObjects2 = jsonTokens2
                    .Where(x => x.JsonValue.GetType() == typeof(JsonObject))
                    .ToArray();

                var jsonObjectGroups2 = jsonObjects2
                    .GroupBy(x => x.Type)
                    .ToArray();

                foreach (var group in jsonObjectGroups2)
                {
                    if (payload.ContainsKey(group.Key))
                    {
                        throw new Exception($"Can't add two claims where one is a JSON object and the other is not a JSON object ({group.Key})");
                    }

                    if (group.Skip(1).Any())
                    {
                        // add as array
                        payload.Add(group.Key, group
                            .Select(x => x.JsonValue.Deserialize<JsonValue>())
                            .ToArray());
                    }
                    else
                    {
                        // add just one
                        payload.Add(group.Key, group.First().JsonValue.Deserialize<JsonValue>());
                    }
                }

                var jsonArrays2 = jsonTokens2
                    .Where(x => x.JsonValue.GetType() == typeof(JsonArray))
                    .ToArray();

                var jsonArrayGroups2 = jsonArrays2
                    .GroupBy(x => x.Type)
                    .ToArray();

                foreach (var group in jsonArrayGroups2)
                {
                    if (payload.ContainsKey(group.Key))
                    {
                        throw new Exception(
                            $"Can't add two claims where one is a JSON array and the other is not a JSON array ({group.Key})");
                    }

                    var newArr = new List<JsonValue>();
                    foreach (var arrays in group)
                    {
                        var arr = arrays.JsonValue.AsArray()
                            .Select(x => x.Deserialize<JsonValue>());
                        newArr.AddRange(arr);
                    }

                    // add just one array for the group/key/claim type
                    payload.Add(group.Key, newArr);
                }

                var unsupportedJsonTokens = jsonTokens2.Except(jsonObjects2)
                    .Except(jsonObjects2)
                    .Except(jsonArrays2)
                    .ToArray();

                var unsupportedJsonClaimTypes = unsupportedJsonTokens
                    .Select(x => x.Type)
                    .Distinct()
                    .ToArray();

                if (unsupportedJsonClaimTypes.Any())
                {
                    throw new Exception(
                        $"Unsupported JSON type for claim types: {unsupportedJsonClaimTypes.Aggregate((x, y) => x + ", " + y)}");
                }

                return payload;
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "Error creating a JSON valued claim");
                throw;
            }
        }
    }
}