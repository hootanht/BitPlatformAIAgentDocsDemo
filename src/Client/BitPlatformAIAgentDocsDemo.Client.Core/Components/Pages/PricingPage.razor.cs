namespace BitPlatformAIAgentDocsDemo.Client.Core.Components.Pages;

public partial class PricingPage
{
    protected override string? Title => "Pricing";
    protected override string? Subtitle => "Choose your plan";

    [AutoInject] private NavigationManager navigationManager = default!;
    [AutoInject] private IExternalNavigationService externalNavigationService = default!;

    private void NavigateToPayment(int planNumber)
    {
        // Determine the plan name and destination based on plan number
        string planName;
        string destination;

        switch (planNumber)
        {
            case 1: // Basic plan
                planName = "basic";
                destination = $"/payment?plan={planName}";
                break;
            case 2: // Pro plan
                planName = "pro";
                destination = $"/payment?plan={planName}";
                break;
            case 3: // Enterprise plan
                planName = "enterprise";
                destination = $"/payment?plan={planName}";
                break;
            default:
                planName = "unknown";
                destination = "/pricing"; // Return to pricing page if unknown plan
                break;
        }

        // Navigate to the appropriate destination
        navigationManager.NavigateTo(destination);
    }
}
