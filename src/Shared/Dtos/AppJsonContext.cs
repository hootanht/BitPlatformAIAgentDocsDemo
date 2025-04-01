using Fido2NetLib;
using Fido2NetLib.Objects;
using BitPlatformAIAgentDocsDemo.Shared.Dtos.Identity;
using BitPlatformAIAgentDocsDemo.Shared.Dtos.Statistics;

namespace BitPlatformAIAgentDocsDemo.Shared.Dtos;

/// <summary>
/// https://devblogs.microsoft.com/dotnet/try-the-new-system-text-json-source-generator/
/// </summary>
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(Dictionary<string, JsonElement>))]
[JsonSerializable(typeof(Dictionary<string, string?>))]
[JsonSerializable(typeof(TimeSpan))]
[JsonSerializable(typeof(string[]))]
[JsonSerializable(typeof(Guid[]))]
[JsonSerializable(typeof(GitHubStats))]
[JsonSerializable(typeof(NugetStatsDto))]
[JsonSerializable(typeof(AppProblemDetails))]
[JsonSerializable(typeof(AssertionOptions))]
[JsonSerializable(typeof(AuthenticatorAssertionRawResponse))]
[JsonSerializable(typeof(AuthenticatorAttestationRawResponse))]
[JsonSerializable(typeof(CredentialCreateOptions))]
[JsonSerializable(typeof(VerifyAssertionResult))]
[JsonSerializable(typeof(VerifyWebAuthnAndSignInDto))]
[JsonSerializable(typeof(WebAuthnAssertionOptionsRequestDto))]
public partial class AppJsonContext : JsonSerializerContext
{
}
