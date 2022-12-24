// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace IdentityServer7.Logging
{
    /// <summary>
    /// Helper to JSON serialize object data for logging.
    /// </summary>
    public static class LogSerializer
    {
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            //IgnoreNullValues = true,
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = true,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.CjkUnifiedIdeographs)
        };

        static LogSerializer()
        {
            Options.Converters.Add(new JsonStringEnumConverter());
        }

        /// <summary>
        /// Serializes the specified object.
        /// </summary>
        /// <param name="logObject">The object.</param>
        /// <returns></returns>
        public static string Serialize(object logObject)
        {
            return JsonSerializer.Serialize(logObject, Options);
        }
    }
}