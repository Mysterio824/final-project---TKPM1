using System.Net;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using DevTools.Application.Exceptions;

namespace DevTools.API.Configurations
{
    public static class KestrelConfig
    {
        public static void ConfigureKestrel(WebApplicationBuilder builder)
        {
            var httpsPort = Environment.GetEnvironmentVariable("APP_HTTPS_PORT") ?? "5000";
            var certPath = Environment.GetEnvironmentVariable("ASPNETCORE_Kestrel__Certificates__Default__Path");
            var certPassword = Environment.GetEnvironmentVariable("ASPNETCORE_Kestrel__Certificates__Default__Password");

            if (!int.TryParse(httpsPort, out var httpsPortNumber) || httpsPortNumber < 1 || httpsPortNumber > 65535)
            {
                throw new BadRequestException($"Invalid APP_HTTPS_PORT value: {httpsPort}. Must be between 1 and 65535.");
            }

            builder.WebHost.ConfigureKestrel(options =>
            {
                if (!string.IsNullOrEmpty(certPath) && !string.IsNullOrEmpty(certPassword))
                {
                    options.Listen(IPAddress.Any, httpsPortNumber, listenOptions =>
                    {
                        listenOptions.UseHttps(certPath, certPassword, options =>
                        {
                            options.ClientCertificateMode = ClientCertificateMode.NoCertificate;
                        });
                        listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
                    });
                }
                else
                {
                    Console.WriteLine("⚠️ Warning: No HTTPS certificate configured. Running in HTTP-only mode.");
                }
            });
        }
    }
}
