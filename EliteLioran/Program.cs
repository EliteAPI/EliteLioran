using System.IO;

using EliteAPI;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using System.Threading.Tasks;

using EliteLioran;


using Valsom.Logging.PrettyConsole;
using Valsom.Logging.PrettyConsole.Formats;
using Valsom.Logging.PrettyConsole.Themes;
using Valsom.Logging.File;
using Valsom.Logging.File.Formats;

// Build the host for dependency injection
var host = Host.CreateDefaultBuilder()
    .ConfigureLogging((context, logger) =>
    {
        logger.ClearProviders();
        logger.SetMinimumLevel(LogLevel.Information);
        logger.AddPrettyConsole(ConsoleFormats.Default, ConsoleThemes.Code);
        logger.AddPrettyConsole("EliteLorian", new DirectoryInfo(Directory.GetCurrentDirectory()), FileNamingFormats.Default, FileFormats.Default);
    })

    .ConfigureServices((context, services) =>
    {
        services.AddEliteAPI();
    })

    .Build();

var core = ActivatorUtilities.CreateInstance<Core>(host.Services);

await core.Run();

await Task.Delay(-1);