﻿// See https://aka.ms/new-console-template for more information
using System;
using AdmailLdapService;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.EntityFrameworkCore;
using AdmailLdapService.DAL.DataAccess;
using AdmailLdapService.DAL.Interfaces;
using AdmailLdapService.DAL.Respositories;
using Serilog;
using Microsoft.Extensions.Logging;
using AdmailLdapService.BL;

var configuration = new ConfigurationBuilder()
     .SetBasePath(Directory.GetCurrentDirectory())  // base path = bin/Debug/...
     .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
     .Build();
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .CreateLogger();



var services = new ServiceCollection();
services.AddLogging(loggingBuilder =>
{
    loggingBuilder.ClearProviders();
    loggingBuilder.AddSerilog();
});

services.AddDbContext<AdmailDbContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("DbConnectionString")));



services.AddScoped<LdapServiceMain>();
services.AddScoped<LdapService>();
services.AddScoped<ITblAdministrationRepository, TblAdministrationRepository>();
services.AddScoped<IUsersRepository, UsersRepository>();


var serviceProvider = services.BuildServiceProvider();
var myService = serviceProvider.GetRequiredService<LdapServiceMain>();

myService.Run();
