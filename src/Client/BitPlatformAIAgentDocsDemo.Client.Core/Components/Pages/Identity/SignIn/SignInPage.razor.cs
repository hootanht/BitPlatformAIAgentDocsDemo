﻿using Fido2NetLib;
using BitPlatformAIAgentDocsDemo.Shared.Dtos.Identity;
using BitPlatformAIAgentDocsDemo.Shared.Controllers.Identity;

namespace BitPlatformAIAgentDocsDemo.Client.Core.Components.Pages.Identity.SignIn;

public partial class SignInPage
{
    [Parameter, SupplyParameterFromQuery(Name = "return-url")]
    public string? ReturnUrlQueryString { get; set; }

    [Parameter, SupplyParameterFromQuery(Name = "userName")]
    public string? UserNameQueryString { get; set; }

    [Parameter, SupplyParameterFromQuery(Name = "email")]
    public string? EmailQueryString { get; set; }

    [Parameter, SupplyParameterFromQuery(Name = "phoneNumber")]
    public string? PhoneNumberQueryString { get; set; }

    [Parameter, SupplyParameterFromQuery(Name = "otp")]
    public string? OtpQueryString { get; set; }

    [Parameter, SupplyParameterFromQuery(Name = "error")]
    public string? ErrorQueryString { get; set; }


    [AutoInject] private IWebAuthnService webAuthnService = default!;
    [AutoInject] private ILocalHttpServer localHttpServer = default!;
    [AutoInject] private ITelemetryContext telemetryContext = default!;
    [AutoInject] private IIdentityController identityController = default!;
    [AutoInject] private IExternalNavigationService externalNavigationService = default!;


    private bool isWaiting;
    private bool isOtpSent;
    private bool requiresTwoFactor;
    private SignInPanelTab currentSignInPanelTab;
    private readonly SignInRequestDto model = new();
    private AppDataAnnotationsValidator? validatorRef;
    private AuthenticatorAssertionRawResponse? webAuthnAssertion;
    private Action unsubscribeIdentityHeaderBackLinkClicked = default!;


    private const string OtpPayload = nameof(OtpPayload);
    private const string TfaPayload = nameof(TfaPayload);


    protected override async Task OnInitAsync()
    {
        await base.OnInitAsync();

        model.UserName = UserNameQueryString;
        model.Email = EmailQueryString;
        model.PhoneNumber = PhoneNumberQueryString;

        if (string.IsNullOrEmpty(OtpQueryString) is false)
        {
            model.Otp = OtpQueryString;

            if (InPrerenderSession is false &&
                (string.IsNullOrEmpty(model.UserName) is false ||
                 string.IsNullOrEmpty(model.Email) is false ||
                 string.IsNullOrEmpty(model.PhoneNumber) is false))
            {
                await DoSignIn();
            }
        }

        if (string.IsNullOrEmpty(ErrorQueryString) is false)
        {
            SnackBarService.Error(ErrorQueryString);
        }

        unsubscribeIdentityHeaderBackLinkClicked = PubSubService.Subscribe(ClientPubSubMessages.IDENTITY_HEADER_BACK_LINK_CLICKED, async payload =>
        {
            var source = (string?)payload;

            if (source == OtpPayload)
            {
                isOtpSent = false;
                model.Otp = null;
            }

            if (source == TfaPayload)
            {
                webAuthnAssertion = null;
                requiresTwoFactor = false;
                model.TwoFactorCode = null;
            }

            await InvokeAsync(StateHasChanged);

            PubSubService.Publish(ClientPubSubMessages.UPDATE_IDENTITY_HEADER_BACK_LINK, null);
        });
    }


    private async Task DoSignIn()
    {
        if (isWaiting) return;
        if (isOtpSent && string.IsNullOrWhiteSpace(model.Otp)) return;

        isWaiting = true;

        try
        {
            if (requiresTwoFactor && string.IsNullOrWhiteSpace(model.TwoFactorCode)) return;

            if (webAuthnAssertion is null)
            {
                CleanModel();

                if (validatorRef?.EditContext.Validate() is false) return;

                model.DeviceInfo = telemetryContext.Platform;

                requiresTwoFactor = await AuthManager.SignIn(model, CurrentCancellationToken);

                if (requiresTwoFactor)
                {
                    PubSubService.Publish(ClientPubSubMessages.UPDATE_IDENTITY_HEADER_BACK_LINK, TfaPayload);
                }
                else
                {
                    NavigationManager.NavigateTo(ReturnUrlQueryString ?? Urls.HomePage, replace: true);
                }
            }
            else
            {
                var response = await identityController
                    .WithQueryIf(AppPlatform.IsBlazorHybrid, "origin", localHttpServer.Origin)
                    .VerifyWebAuthAndSignIn(new() { ClientResponse = webAuthnAssertion, TfaCode = model.TwoFactorCode }, CurrentCancellationToken);
                await AuthManager.StoreTokens(response!, model.RememberMe);
                NavigationManager.NavigateTo(ReturnUrlQueryString ?? Urls.HomePage, replace: true);
            }
        }
        catch (KnownException e)
        {
            // To disable the sign-in button until a specific time after a user lockout, use the value of `e.TryGetExtensionDataValue<TimeSpan>("TryAgainIn", out var tryAgainIn)`.

            SnackBarService.Error(e.Message);
        }
        finally
        {
            isWaiting = false;
        }
    }

    private async Task HandleOnSocialSignIn(string provider)
    {
        try
        {
            var port = localHttpServer.EnsureStarted();

            var redirectUrl = await identityController.GetSocialSignInUri(provider, ReturnUrlQueryString, port is -1 ? null : port, CurrentCancellationToken);

            await externalNavigationService.NavigateToAsync(redirectUrl);
        }
        catch (KnownException e)
        {
            SnackBarService.Error(e.Message);
        }
    }

    private async Task HandleOnPasswordlessSignIn()
    {
        if (isWaiting) return;
        isWaiting = true;

        try
        {
            var userIds = await webAuthnService.GetWebAuthnConfiguredUserIds();

            if (AppPlatform.IsBlazorHybrid)
            {
                localHttpServer.EnsureStarted();
            }

            var options = await identityController
                .WithQueryIf(AppPlatform.IsBlazorHybrid, "origin", localHttpServer.Origin)
                .GetWebAuthnAssertionOptions(new() { UserIds = userIds }, CurrentCancellationToken);

            try
            {
                webAuthnAssertion = await webAuthnService.GetWebAuthnCredential(options, CurrentCancellationToken);
            }
            catch (Exception ex)
            {
                // we can safely handle the exception thrown here since it mostly because of a timeout or user cancelling the native ui.
                ExceptionHandler.Handle(ex, AppEnvironment.IsDev() ? ExceptionDisplayKind.NonInterrupting : ExceptionDisplayKind.None);
                return;
            }

            var response = await identityController
                .WithQueryIf(AppPlatform.IsBlazorHybrid, "origin", localHttpServer.Origin)
                .VerifyWebAuthAndSignIn(new() { ClientResponse = webAuthnAssertion }, CurrentCancellationToken);

            requiresTwoFactor = response.RequiresTwoFactor;

            if (requiresTwoFactor)
            {
                PubSubService.Publish(ClientPubSubMessages.UPDATE_IDENTITY_HEADER_BACK_LINK, TfaPayload);
            }
            else
            {
                await AuthManager.StoreTokens(response!, model.RememberMe);
                NavigationManager.NavigateTo(ReturnUrlQueryString ?? Urls.HomePage, replace: true);
            }
        }
        catch (KnownException e)
        {
            webAuthnAssertion = null;
            SnackBarService.Error(e.Message);
        }
        finally
        {
            isWaiting = false;
        }
    }

    private void HandleOnSignInPanelTabChange(SignInPanelTab tab)
    {
        currentSignInPanelTab = tab;
    }

    private Task HandleOnSendOtp() => SendOtp(false);
    private Task HandleOnResendOtp() => SendOtp(true);
    private async Task SendOtp(bool resend)
    {
        try
        {
            CleanModel();

            if (model.Email is null && model.PhoneNumber is null) return;

            if (model.Email is not null && new EmailAddressAttribute().IsValid(model.Email) is false)
            {
                SnackBarService.Error(string.Format(AppStrings.EmailAddressAttribute_ValidationError, AppStrings.Email));
                return;
            }

            if (model.PhoneNumber is not null && new PhoneAttribute().IsValid(model.PhoneNumber) is false)
            {
                SnackBarService.Error(string.Format(AppStrings.PhoneAttribute_ValidationError, AppStrings.PhoneNumber));
                return;
            }

            var request = new IdentityRequestDto { UserName = model.UserName, Email = model.Email, PhoneNumber = model.PhoneNumber };

            await identityController.SendOtp(request, ReturnUrlQueryString, CurrentCancellationToken);

            if (resend is false)
            {
                isOtpSent = true;

                PubSubService.Publish(ClientPubSubMessages.UPDATE_IDENTITY_HEADER_BACK_LINK, OtpPayload);
            }
        }
        catch (KnownException e)
        {
            SnackBarService.Error(e.Message);
        }
    }

    private async Task HandleOnSendTfaToken()
    {
        try
        {
            if (webAuthnAssertion is null)
            {
                CleanModel();

                await identityController.SendTwoFactorToken(model, CurrentCancellationToken);
            }
            else
            {
                await identityController
                    .WithQueryIf(AppPlatform.IsBlazorHybrid, "origin", localHttpServer.Origin)
                    .VerifyWebAuthAndSendTwoFactorToken(webAuthnAssertion, CurrentCancellationToken);
            }

            SnackBarService.Success(Localizer[nameof(AppStrings.TfaTokenSentMessage)]);
        }
        catch (KnownException e)
        {
            SnackBarService.Error(e.Message);
        }
    }

    private void CleanModel()
    {
        if (currentSignInPanelTab is SignInPanelTab.Email)
        {
            model.PhoneNumber = null;
            if (validatorRef is null) return;

            validatorRef.EditContext.NotifyFieldChanged(validatorRef.EditContext.Field(nameof(SignInRequestDto.PhoneNumber)));
        }
        else
        {
            model.Email = null;
            if (validatorRef is null) return;

            validatorRef.EditContext.NotifyFieldChanged(validatorRef.EditContext.Field(nameof(SignInRequestDto.Email)));
        }
    }

    protected override async ValueTask DisposeAsync(bool disposing)
    {
        unsubscribeIdentityHeaderBackLinkClicked?.Invoke();
        await base.DisposeAsync(disposing);
    }
}
