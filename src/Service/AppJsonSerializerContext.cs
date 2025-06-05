namespace Innago.Shared.ReplaceMe;

using System.Text.Json.Serialization;

[JsonSerializable(typeof(Dictionary<string, string>))]
internal partial class AppJsonSerializerContext : JsonSerializerContext;