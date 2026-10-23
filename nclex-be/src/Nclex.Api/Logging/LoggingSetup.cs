using System.Globalization;
using Serilog;
using Serilog.Formatting.Compact;

namespace Nclex.Api.Logging;

public static class LoggingSetup
{
    /// <summary>
    /// Readable text locally; structured JSON everywhere else so log tooling can index
    /// requestId and other properties (architecture.md section 9).
    /// </summary>
    public static LoggerConfiguration WriteToConsole(this LoggerConfiguration logger, IHostEnvironment environment) =>
        environment.IsDevelopment()
            ? logger.WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}",
                formatProvider: CultureInfo.InvariantCulture)
            : logger.WriteTo.Console(new RenderedCompactJsonFormatter());
}
