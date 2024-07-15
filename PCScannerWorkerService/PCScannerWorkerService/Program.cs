
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PCScannerWorkerService;
using SchoolPCScanner.Models;
using SchoolPCScanner.Services;
using SchoolPCScanner.Services.Interfaces;
using System.Reflection;


//var builder = Host.CreateApplicationBuilder(args);
//builder.Services.AddHostedService<Worker>();
var builder = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((hostContext, config) =>
    {
        // Add the configuration from the appsettings.json and the environment variables
        config.SetBasePath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).AddJsonFile("appsettingsWS.json", optional: true, reloadOnChange: true);
        //config.AddJsonFile("appsettingsWS.json", optional: true, reloadOnChange: true);
        config.AddEnvironmentVariables();
    })
    .UseWindowsService()
    .ConfigureServices((hostContext, services) =>
    {
        var connectionString = hostContext.Configuration.GetConnectionString("DefaultConnection");
        
        // Add the database context to the DI container
        services.AddDbContext<SchoolPCScannerDbContext>(options =>
            options.UseSqlServer(connectionString));

        // Voeg ProxyClient toe aan de DI-container
        services.AddTransient<ProxyClient.V3PortClient>();

        // Configure SMTP settings
        services.Configure<SmtpSettings>(hostContext.Configuration.GetSection("Smtp"));
        // Voeg EmailService toe aan de DI-container
        services.AddTransient<IEmailService, EmailService>();


        // Voeg de hosted service (Worker) toe aan de DI-container
        services.AddHostedService<Worker>();
    })
    
    .ConfigureLogging((hostingContext, logging) =>
    {
        // Remove the default console logger
        logging.ClearProviders();
        // Add the console logger with the configuration from the appsettings.json
        logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
        // Add the console logger
        logging.AddConsole();

        // Add EventLog logging (only on windows)
        logging.AddEventLog(eventLogSettings =>
        {
            eventLogSettings.SourceName = "PCScannerWorkerService";
        });
    });

//  Create the host
var host = builder.Build();
host.Run();
