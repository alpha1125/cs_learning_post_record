using Serilog;
using Serilog.Exceptions;
using DotNetEnv;
using System.Security.Cryptography.X509Certificates;

var builder = WebApplication.CreateBuilder(args);

// Load environment variables
Env.Load();

var loggingPath = Environment.GetEnvironmentVariable("LOGGING_PATH");
if (string.IsNullOrEmpty(loggingPath))
{
    Console.WriteLine("The LOGGING_PATH environment variable is not set. Please check your .env file.");
    return;
}

if (!Directory.Exists(loggingPath))
{
    Directory.CreateDirectory(loggingPath);
}

// Check if the directory is writable
try
{
    using (File.Create(Path.Combine(loggingPath, "temp.txt"))) { }
    File.Delete(Path.Combine(loggingPath, "temp.txt"));
}
catch (UnauthorizedAccessException)
{
    Console.WriteLine($"The directory {loggingPath} is not writable");
    return;
}

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithExceptionDetails()
    .WriteTo.Console(
        outputTemplate:
        "DISPLAY [{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} <s:{SourceContext}>{NewLine}{Exception}")
    .WriteTo.File(Path.Combine(loggingPath, "log-.txt"), rollingInterval: RollingInterval.Day,
        outputTemplate: "WRITE [{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} <s:{SourceContext}>{NewLine}{Exception}")
    .CreateLogger();

try
{
    Log.Information("Starting up");

    // Load the certificate
    string p12FilePath = Environment.GetEnvironmentVariable("P12_FILE_PATH");
    string p12Password = Environment.GetEnvironmentVariable("P12_PASSWORD");

    if (string.IsNullOrEmpty(p12FilePath))
    {
        throw new Exception("P12_FILE_PATH environment variable is not set. Please check your .env file.");
    }

    X509Certificate2 certificate;
    if (string.IsNullOrEmpty(p12Password))
    {
        certificate = new X509Certificate2(p12FilePath);
    }
    else
    {
        certificate = new X509Certificate2(p12FilePath, p12Password);
    }

    // Configure Kestrel to use the certificate
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.ListenAnyIP(5001, listenOptions =>
        {
            listenOptions.UseHttps(certificate);
        });
    });

    builder.Services.AddControllers();
    builder.Services.AddAuthorization();
    builder.Host.UseSerilog();

    var app = builder.Build();

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error");
        app.UseHsts();
    }

    app.UseRouting();
    app.UseHttpsRedirection();
    app.UseAuthorization();
    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application start-up failed");
}
finally
{
    Log.CloseAndFlush();
}
