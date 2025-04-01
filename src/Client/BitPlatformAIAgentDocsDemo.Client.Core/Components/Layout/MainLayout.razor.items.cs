namespace BitPlatformAIAgentDocsDemo.Client.Core.Components.Layout;

public partial class MainLayout
{
    private List<BitNavItem> navPanelAuthenticatedItems = [];
    private List<BitNavItem> navPanelUnAuthenticatedItems = [];

    [AutoInject] protected IStringLocalizer<AppStrings> localizer = default!;

    private void InitializeNavPanelItems()
    {
        BitNavItem homeNavItem = new()
        {
            Text = localizer[nameof(AppStrings.Home)],
            IconName = BitIconName.Home,
            Url = Urls.HomePage,
        };

        BitNavItem termsNavItem = new()
        {
            Text = localizer[nameof(AppStrings.Terms)],
            IconName = BitIconName.EntityExtraction,
            Url = Urls.TermsPage,
        };

        BitNavItem pricingNavItem = new()
        {
            Text = "Pricing",
            IconName = BitIconName.Money,
            Url = Urls.PricingPage,
        };

        navPanelUnAuthenticatedItems = [homeNavItem, termsNavItem, pricingNavItem];

        navPanelAuthenticatedItems =
        [
            homeNavItem,
            termsNavItem,
            pricingNavItem
        ];

        BitNavItem aboutNavItem = new()
        {
            Text = localizer[nameof(AppStrings.About)],
            IconName = BitIconName.Info,
            Url = Urls.AboutPage,
        };


        navPanelAuthenticatedItems.Add(aboutNavItem);
        navPanelUnAuthenticatedItems.Add(aboutNavItem);

        navPanelAuthenticatedItems.Add(new()
        {
            Text = localizer[nameof(AppStrings.Settings)],
            IconName = BitIconName.Equalizer,
            Url = Urls.SettingsPage,
            AdditionalUrls =
            [
                $"{Urls.SettingsPage}/{Urls.SettingsSections.Profile}",
                $"{Urls.SettingsPage}/{Urls.SettingsSections.Account}",
                $"{Urls.SettingsPage}/{Urls.SettingsSections.Tfa}",
                $"{Urls.SettingsPage}/{Urls.SettingsSections.Sessions}",
            ]
        });
    }
}
