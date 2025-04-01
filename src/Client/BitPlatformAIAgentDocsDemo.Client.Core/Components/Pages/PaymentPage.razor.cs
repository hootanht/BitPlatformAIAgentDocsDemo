namespace BitPlatformAIAgentDocsDemo.Client.Core.Components.Pages;

public partial class PaymentPage
{
    [Parameter, SupplyParameterFromQuery(Name = "plan")]
    public string? PlanType { get; set; }

    private bool isProcessing = false;
    private bool isSuccess = false;
    private string planName = "Basic";
    private readonly PaymentInfo paymentInfo = new();

    protected override Task OnInitAsync()
    {
        if (!string.IsNullOrEmpty(PlanType))
        {
            planName = char.ToUpper(PlanType[0]) + PlanType[1..];
        }
        return base.OnInitAsync();
    }

    private string GetPlanPrice()
    {
        return planName.ToLower() switch
        {
            "basic" => "$9",
            "pro" => "$29",
            "enterprise" => "$99",
            _ => "$9"
        };
    }

    private async Task ProcessPayment()
    {
        isProcessing = true;

        // Simulate a payment processing delay
        await Task.Delay(2000);

        isProcessing = false;
        isSuccess = true;
        StateHasChanged();
    }

    private void ReturnToHome()
    {
        NavigationManager.NavigateTo(Urls.HomePage);
    }
}

public class PaymentInfo
{
    [Required(ErrorMessage = "Cardholder name is required")]
    public string CardholderName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Card number is required")]
    [RegularExpression(@"^\d{16}$", ErrorMessage = "Card number must be 16 digits")]
    public string CardNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Expiry date is required")]
    [RegularExpression(@"^(0[1-9]|1[0-2])\/([0-9]{2})$", ErrorMessage = "Expiry date must be in MM/YY format")]
    public string ExpiryDate { get; set; } = string.Empty;

    [Required(ErrorMessage = "CVV is required")]
    [RegularExpression(@"^\d{3,4}$", ErrorMessage = "CVV must be 3 or 4 digits")]
    public string CVV { get; set; } = string.Empty;
}
