﻿namespace BitPlatformAIAgentDocsDemo.Shared;

public static partial class Urls
{
    public const string HomePage = "/";

    public const string NotFoundPage = "/not-found";

    public const string TermsPage = "/terms";

    public const string SettingsPage = "/settings";

    public const string AboutPage = "/about";

    public const string PricingPage = "/pricing";

    public const string Authorize = "/authorize";

    public static readonly string[] All = typeof(Urls).GetFields()
                                                      .Where(f => f.FieldType == typeof(string))
                                                      .Select(f => f.GetValue(null)!.ToString()!)
                                                      .ToArray();
}
