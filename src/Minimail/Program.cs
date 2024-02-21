using System.Collections.Concurrent;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using Minimail;
using Minimail.Components;
using Minimail.Core;
using MudBlazor;
using MudBlazor.Services;
using SmtpServer;

// https://www.checktls.com/TestReceiver
// https://dnschecker.org/#A/m1.apollo3zehn.net
// https://www.psw-group.de/blog/mta-sts-gestaltet-mail-versand-und-empfang-sicherer/7080
// https://www.stevenrombauts.be/2018/12/test-smtp-with-telnet-or-openssl/
// https://github.com/cosullivan/SmtpServer/blob/master/Examples/SampleApp/Examples/CommonPortsExample.cs
// https://de.wikipedia.org/wiki/STARTTLS

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.Configure<GeneralOptions>(
    builder.Configuration.GetSection(GeneralOptions.Section));

builder.Services.Configure<PathsOptions>(
    builder.Configuration.GetSection(PathsOptions.Section));

builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.VisibleStateDuration = 4000;
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomCenter;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

RunSmtpServer();

app.Run();

void RunSmtpServer()
{
    Console.WriteLine("Welcome to Minimail");

    // configuration
    var configuration = MinimailOptionsBase.BuildConfiguration(args);

    var generalOptions = new GeneralOptions();

    configuration
        .GetSection(GeneralOptions.Section)
        .Bind(generalOptions);

    var pathsOptions = new PathsOptions();

    configuration
        .GetSection(PathsOptions.Section)
        .Bind(pathsOptions);

    // logging
    var loggerFactory = LoggerFactory.Create(
        builder => builder.AddConsole());

    var logger = loggerFactory.CreateLogger("SmtpServer");

    // whitelist
    if (File.Exists(pathsOptions.Whitelist))
    {
        var jsonString = File.ReadAllText(pathsOptions.Whitelist);
        
        State.Whitelist = JsonSerializer.Deserialize<ConcurrentDictionary<string, object?>>(jsonString) 
            ?? throw new Exception("Unable to read the whitelist file.");
    }
    else
    {
        State.Whitelist = new();
    }

    // smtp server
    var smtpOptions = new SmtpServerOptionsBuilder()
        .ServerName(generalOptions.Domain)

        // Provider to Provider transfer
        .Endpoint(builder =>
            builder
                .Port(25)
                .Certificate(X509Certificate2.CreateFromPemFile(pathsOptions.CertFullChain, pathsOptions.CertPrivateKey)))

        // Send mails
        .Endpoint(builder =>
            builder
                .Port(587)
                .AuthenticationRequired(true)
                .AllowUnsecureAuthentication(false)
                .Certificate(X509Certificate2.CreateFromPemFile(pathsOptions.CertFullChain, pathsOptions.CertPrivateKey)))
        .Build();

    var serviceProvider = new SmtpServer.ComponentModel.ServiceProvider();
    serviceProvider.Add(new MiniMailboxFilter(State.Whitelist, logger));
    serviceProvider.Add(new MiniMessageStore(pathsOptions, logger));

    var smtpServer = new SmtpServer.SmtpServer(smtpOptions, serviceProvider);

    _ = Task.Run(async () =>
    {
        try
        {
            await smtpServer.StartAsync(CancellationToken.None);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unable to start smtp server.");
            Environment.Exit(-1);
        }
    });
}