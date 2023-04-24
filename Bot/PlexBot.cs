// LFInteractive LLC. - All Rights Reserved

using Chase.Plex_Bot.Bot.Commands;
using Chase.Plex_Bot.Core;
using Chase.Plex_Bot.Core.Controller;
using Chase.Plex_Bot.Sonarr.Controller;
using Discord;
using Discord.WebSocket;
using Serilog;

namespace Chase.Bot;

internal class PlexBot
{
    public static DiscordSocketClient client = new(new() { UseInteractionSnowflakeDate = false, ConnectionTimeout = 30 * 1000 });

    public async Task MainAsync()
    {
        if (!ConfigController.Instance.Validate())
        {
            Log.Fatal("Config is not VALID!");
            return;
        }

        try
        {
            RadarrController.Init();
            SonarrController.Init();
        }
        catch
        {
            return;
        }
        client.Log += Task (msg) =>
        {
            Log.Information(msg.ToString());
            return Task.CompletedTask;
        };
        client.Ready += async Task () =>
        {
            try
            {
                await SlashMovieCommand.Register();
                await SlashTVCommand.Register();
            }
            catch (Exception e)
            {
                Log.Error("Unable to create TV Slash Command: {ERROR}", e.Message, e);
            }
        };

        Log.Information("Logging PlexBot in...");
        await client.LoginAsync(TokenType.Bot, ConfigController.Instance.Get.DiscordToken);
        await client.StartAsync();

        Console.CancelKeyPress += (s, e) => { OnExit(); };
        AppDomain.CurrentDomain.ProcessExit += (s, e) => { OnExit(); };

        await Task.Delay(-1);
    }

    private static async Task Main()
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console(Serilog.Events.LogEventLevel.Information)
            .WriteTo.File(Path.Combine(Values.Directories.Logs, "latest.log"), Serilog.Events.LogEventLevel.Information, rollingInterval: RollingInterval.Day, buffered: true)
            .WriteTo.File(Path.Combine(Values.Directories.Logs, "debug.log"), Serilog.Events.LogEventLevel.Debug, rollingInterval: RollingInterval.Day, buffered: true)
            .MinimumLevel.Debug()
            .CreateLogger();

        await new PlexBot().MainAsync();

        Log.CloseAndFlush();
    }

    private static void OnExit()
    {
        if (client != null)
        {
            Log.Information("Logging PlexBot out...");
            client.LogoutAsync();
        }
        Log.Warning("Exiting...");
        Environment.Exit(0);
    }
}