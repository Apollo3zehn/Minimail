using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Minimail.Core;
using SmtpServer;
using SmtpServer.ComponentModel;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Minimail
{
    // https://www.checktls.com/TestReceiver
    // https://dnschecker.org/#A/m1.apollo3zehn.net
    // https://www.psw-group.de/blog/mta-sts-gestaltet-mail-versand-und-empfang-sicherer/7080
    // https://www.stevenrombauts.be/2018/12/test-smtp-with-telnet-or-openssl/
    // https://github.com/cosullivan/SmtpServer/blob/master/Examples/SampleApp/Examples/CommonPortsExample.cs
    // https://de.wikipedia.org/wiki/STARTTLS

    public class Program
    {
        public static ConcurrentDictionary<string, object> _whitelist;

        public static async Task Main(string[] args)
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
                _whitelist = JsonSerializer.Deserialize<ConcurrentDictionary<string, object>>(jsonString);
            }
            else
            {
                _whitelist = new ConcurrentDictionary<string, object>();
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

            var serviceProvider = new ServiceProvider();
            serviceProvider.Add(new MiniMailboxFilter(_whitelist, logger));
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
            
            // blazor
            Program.CreateHostBuilder(args, configuration).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args, IConfiguration configuration) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.ConfigureLogging(builder => builder.AddConsole());
                    webBuilder.UseConfiguration(configuration);
                    webBuilder.UseUrls("http://0.0.0.0:8080");
                });
    }
}
