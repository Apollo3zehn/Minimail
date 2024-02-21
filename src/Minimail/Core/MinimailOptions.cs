using System.Runtime.InteropServices;

namespace Minimail.Core;

public abstract record MinimailOptionsBase()
{
    internal static IConfiguration BuildConfiguration(string[] args)
    {
        var builder = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json");

        var settingsPath = Environment.GetEnvironmentVariable("MINIMAIL_PATHS__SETTINGS");

        if (settingsPath is null)
            settingsPath = PathsOptions.DefaultSettingsPath;

        if (settingsPath.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            builder.AddJsonFile(settingsPath, optional: true);

        builder
            .AddEnvironmentVariables(prefix: "MINIMAIL_")
            .AddCommandLine(args);

        return builder.Build();
    }
}

public record GeneralOptions
{
    public const string Section = "General";

    public string Domain { get; set; } = "minimail.org";
}

public record PathsOptions
{
    public const string Section = "Paths";

    public string Maildir { get; set; } = Path.Combine(DefaultRootPath, "Maildir");

    public string CertFullChain { get; set; } = Path.Combine(DefaultRootPath, "fullchain.pem");

    public string CertPrivateKey { get; set; } = Path.Combine(DefaultRootPath, "privkey.pem");

    public string Whitelist { get; set; } = Path.Combine(DefaultRootPath, "whitelist.json");

    private static string DefaultRootPath { get; } = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
        ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Minimail")
        : "/var/lib/minimail";

    public static string DefaultSettingsPath { get; } = Path.Combine(DefaultRootPath, "settings.json");
}