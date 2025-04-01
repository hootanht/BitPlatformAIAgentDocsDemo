using Fido2NetLib;
using BitPlatformAIAgentDocsDemo.Shared.Dtos.Statistics;

namespace BitPlatformAIAgentDocsDemo.Server.Api.Services;

/// <summary>
/// https://devblogs.microsoft.com/dotnet/try-the-new-system-text-json-source-generator/
/// </summary>
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(NugetStatsDto))]
[JsonSerializable(typeof(ProblemDetails))]
[JsonSerializable(typeof(AuthenticatorResponse))]
public partial class ServerJsonContext : JsonSerializerContext
{
}
