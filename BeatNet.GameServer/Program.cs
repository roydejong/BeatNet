using System.Diagnostics;
using System.Reflection;
using BeatNet.GameServer;
using BeatNet.GameServer.Main;
using BeatNet.GameServer.Management;
using BeatNet.Lib;
using BeatNet.Lib.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

// ---------------------------------------------------------------------------------------------------------------------

var selfVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion;

Console.Title = "BeatNet Server";
Console.ForegroundColor = ConsoleColor.Magenta;
Console.WriteLine($"BeatNet Server v{selfVersion} (for Beat Saber {VersionConsts.GameVersionMinimum}+)");
Console.ResetColor();
Console.WriteLine("https://github.com/roydejong/BeatNet");

// ---------------------------------------------------------------------------------------------------------------------

var log = Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .CreateLogger();

// ---------------------------------------------------------------------------------------------------------------------

using var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddSingleton(Config.LoadOrInitializeFile("config/server.json"));
        services.AddHostedService<BeatSaberService>();
    })
    .UseSerilog(log)
    .UseSystemd()
    .UseConsoleLifetime()
    .Build();

await host.RunAsync();