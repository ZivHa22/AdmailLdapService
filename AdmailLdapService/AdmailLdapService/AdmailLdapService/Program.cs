// See https://aka.ms/new-console-template for more information
using System;
using AdmailLdapService;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

using Serilog;
using Microsoft.Extensions.Logging;
using AdmailLdapService.BL;

var configuration = new ConfigurationBuilder()
     .SetBasePath(Directory.GetCurrentDirectory())  // base path = bin/Debug/...
     .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
     .Build();


Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.File(
        path: "Logs/log-.txt",                // Base name
        rollingInterval: RollingInterval.Day, // Creates log-2025-11-11.txt, etc.
        retainedFileCountLimit: 14,           // Keep 14 days of logs (optional)
        shared: true,                         // Allow shared access
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
    )
    .CreateLogger();


var services = new ServiceCollection();
services.AddLogging(loggingBuilder =>
{
    loggingBuilder.ClearProviders();
    loggingBuilder.AddSerilog();
});






services.AddSingleton<IConfiguration>(configuration);

services.AddScoped<LdapServiceMain>();
services.AddScoped<LdapService>();
services.AddScoped<LdapServiceNovell>();
services.AddScoped<SecurityService>();



var serviceProvider = services.BuildServiceProvider();
var myService = serviceProvider.GetRequiredService<LdapServiceMain>();

myService.Run();
