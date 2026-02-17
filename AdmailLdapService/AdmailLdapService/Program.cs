// See https://aka.ms/new-console-template for more information
using System;
using AdmailLdapService;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using AdmailLdapService.DAL.DataAccess;
using AdmailLdapService.DAL.Interfaces;
using AdmailLdapService.DAL.Respositories;
using Microsoft.Data.SqlClient;
using Serilog;
using Microsoft.Extensions.Logging;
using AdmailLdapService.BL;

var configuration = new ConfigurationBuilder()
     .SetBasePath(Directory.GetCurrentDirectory())  // base path = bin/Debug/...
     .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
     .Build();

var logPath = Path.Combine(AppContext.BaseDirectory, "Logs", "log-.txt");
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File(
        path: logPath,             // Base name
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




services.AddDbContext<AdmailDbContext>(options =>
{
    SecurityService securityService = new SecurityService();
    string ConStr = configuration.GetConnectionString("DbConnectionString");
    SqlConnectionStringBuilder sqlConnectionStringBuilder = new SqlConnectionStringBuilder(ConStr);
    string DecryptString = securityService.DecryptString(sqlConnectionStringBuilder.Password);
    sqlConnectionStringBuilder.Password = DecryptString;
    string connection = sqlConnectionStringBuilder.ConnectionString;
    options.UseSqlServer(connection);

});


services.AddSingleton<IConfiguration>(configuration);
services.AddScoped<ITblAdministrationRepository, TblAdministrationRepository>();
services.AddScoped<IUsersRepository, UsersRepository>();
services.AddScoped<LdapService>();
services.AddScoped<LdapServiceMain>();

services.AddScoped<SecurityService>();



var serviceProvider = services.BuildServiceProvider();
var myService = serviceProvider.GetRequiredService<LdapServiceMain>();

myService.Run();
