// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace IdentityServer7.Storage.Stores.Serialization;

/// <summary>
/// JSON-based persisted grant serializer
/// </summary>
/// <seealso cref="IPersistentGrantSerializer" />
public class PersistentGrantSerializer : IPersistentGrantSerializer
{
    private static readonly JsonSerializerOptions Settings;

    /// <summary>
    /// Set this to override JsonExtensions.Serializer and  (using in System.IdentityModel.Tokens.Jwt)
    /// </summary>
    public static bool OverrideSerializer;

    private static readonly Serializer OldSerializer;

    static PersistentGrantSerializer()
    {
        Settings = new JsonSerializerOptions
        {
            //IgnoreReadOnlyFields = true,
            IgnoreReadOnlyProperties = true,
            //DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            //TypeInfoResolver= new CustomContractResolver()
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.CjkUnifiedIdeographs)
        };
        Settings.Converters.Add(new ClaimConverter());
        Settings.Converters.Add(new ClaimsPrincipalConverter());

        OldSerializer = JsonExtensions.Serializer;

        JsonExtensions.Serializer = Serializer;
    }

    private static string Serializer(object obj)
    {
        if (OverrideSerializer == false) return OldSerializer(obj);

        if (obj is Dictionary<string, object> dict) return ToJson(dict);

        var json = JsonSerializer.Serialize(obj, Settings);
        //Console.WriteLine("json:" + json);
        return json;
    }

    /// <summary>
    /// like JwtPayload...
    /// </summary>
    /// <param name="data"></param>
    /// <returns>Json string</returns>
    private static string ToJson(Dictionary<string, object> data)
    {
        var sb = new StringBuilder("{");

        foreach (var key in data.Keys)
        {
            sb.AppendLine();
            sb.Append($"'{key}':");

            var obj = data[key];
            var typeName = obj.GetType().FullName;
            if (typeName.IndexOf("Microsoft.IdentityModel.Json.Linq.") == 0)
            {
                sb.Append(obj.ToString());
            }
            else sb.Append(Serializer(obj));

            sb.Append(',');
        }

        var len = sb.Length;
        if (len > 2)
        {
            sb[len - 1] = '}';
        }
        else sb.Append('}');

        return sb.ToString();
    }

    /// <summary>
    /// Serializes the specified value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value">The value.</param>
    /// <returns></returns>
    public string Serialize<T>(T value) => JsonSerializer.Serialize(value, Settings);

    /// <summary>
    /// Deserializes the specified string.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="json">The json.</param>
    /// <returns></returns>
    public T Deserialize<T>(string json) => JsonSerializer.Deserialize<T>(json, Settings);
}