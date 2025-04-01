using BitPlatformAIAgentDocsDemo.Client.Core.Services.Contracts;
using BitPlatformAIAgentDocsDemo.Server.Api.Data;

namespace BitPlatformAIAgentDocsDemo.Server.Web;

public static partial class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(options: new()
        {
            Args = args,
            ContentRootPath = AppContext.BaseDirectory
        });

        AppEnvironment.Set(builder.Environment.EnvironmentName);

        builder.Configuration.AddClientConfigurations(clientEntryAssemblyName: "BitPlatformAIAgentDocsDemo.Client.Web");


        // The following line (using the * in the URL), allows the emulators and mobile devices to access the app using the host IP address.
        if (builder.Environment.IsDevelopment() && AppPlatform.IsWindows)
        {
            builder.WebHost.UseUrls("http://localhost:9670", "http://*:9670");
        }

        builder.AddServerWebProjectServices();

        var app = builder.Build();

        AppDomain.CurrentDomain.UnhandledException += (_, e) => LogException(e.ExceptionObject, reportedBy: nameof(AppDomain.UnhandledException), app);
        TaskScheduler.UnobservedTaskException += (_, e) => { LogException(e.Exception, reportedBy: nameof(TaskScheduler.UnobservedTaskException), app); e.SetObserved(); };

        if (builder.Environment.IsDevelopment())
        {
            await using var scope = app.Services.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await dbContext.Database.EnsureCreatedAsync();
        }

        app.ConfigureMiddlewares();

        await app.RunAsync();
    }

    private static void LogException(object? error, string reportedBy, WebApplication app)
    {
        if (error is Exception exp)
        {
            using var scope = app.Services.CreateScope();
            scope.ServiceProvider.GetRequiredService<IExceptionHandler>().Handle(exp, parameters: new()
            {
                { nameof(reportedBy), reportedBy }
            }, displayKind: AppEnvironment.IsDev() ? ExceptionDisplayKind.NonInterrupting : ExceptionDisplayKind.None);
        }
        else
        {
            _ = Console.Error.WriteLineAsync(error?.ToString() ?? "Unknown error");
        }
    }
}
