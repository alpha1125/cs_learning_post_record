using Serilog;
using Microsoft.AspNetCore.Mvc;
using ILogger = Serilog.ILogger;

namespace cs_ping_logger
{
    [Route("api/ping")]
    [ApiController]
    public class Ping : ControllerBase
    {
        private readonly ILogger _logger;

        public Ping()
        {
            _logger = Log.ForContext<Ping>();
        }

        [HttpGet]
        public IActionResult Get()
        {
            _logger.Information("Get method called");
            return Ok(new { status = "ok" });
        }
        
        [HttpPost]
        public async Task<IActionResult> Post()
        {
            using (StreamReader reader = new StreamReader(Request.Body))
            {
                string rawJson = await reader.ReadToEndAsync();

                // Generate the filename with the current date and hour
                // string logDirectory = "/tmp/var/log/ping";
                // string timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd-HH");
                // string logFilename = $"payload-{timestamp}.log";
                // string logFilePath = Path.Combine(logDirectory, logFilename);
                //
                // // Check if the directory exists
                // await System.IO.File.AppendAllTextAsync(logFilePath, rawJson + "\n");

                // Log the received JSON (optional)
                // System.Console.WriteLine(rawJson);

                return Ok(new { message = "JSON received and logged successfully", json = rawJson });
            }
        }
    }
}