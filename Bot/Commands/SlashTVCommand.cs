// LFInteractive LLC. - All Rights Reserved
using Chase.Bot;
using Chase.Plex_Bot.Sonarr.Controller;
using Chase.Plex_Bot.Sonarr.Model;
using Discord;
using Discord.WebSocket;

namespace Chase.Plex_Bot.Bot.Commands;

internal static class SlashTVCommand
{
    public static Dictionary<SocketUser, SeriesModel> SearchResult = new();

    public static async Task Handle(SocketSlashCommand command)
    {
        string method = command.Data.Options.First().Name;
        string query = command.Data.Options.First().Options.First().Value.ToString() ?? "";
        switch (method)
        {
            case "search":
                SeriesModel[] results = SonarrController.Search(query);
                if (!results.Any())
                {
                    await command.RespondAsync($"No TV Show found with title of \"{query}\"", ephemeral: true);
                    break;
                }
                SeriesModel result = results[0];
                if (SearchResult.ContainsKey(command.User))
                {
                    SearchResult.Remove(command.User);
                }
                SearchResult.Add(command.User, result);
                EmbedBuilder embedBuilder = new EmbedBuilder()
                    .WithTitle($"{result.Title} - {(result.Added ? "On Plex" : "Not Added!")}")
                    .WithImageUrl(result.Images.Poster.ToString())
                    .WithColor(Color.Green)
                    .WithCurrentTimestamp()
                    .WithDescription(result.Overview)
                    ;
                if (!result.Added)
                {
                    var addButton = new ComponentBuilder()
                        .WithButton($"Add {result.Title}", "add-series-button-id")
                        ;

                    await command.RespondAsync(embed: embedBuilder.Build(), components: addButton.Build(), ephemeral: true);
                }
                else
                {
                    await command.RespondAsync(embed: embedBuilder.Build(), ephemeral: true);
                }
                break;
        }
    }

    public static async Task Register()
    {
        SlashCommandBuilder tvCommand = new SlashCommandBuilder()
            .WithName("tv")
            .WithDescription("Handles TV Series")
            .AddOptions(new SlashCommandOptionBuilder()
                .WithName("search")
                .WithDescription("Searches for TV Show")
                .WithType(ApplicationCommandOptionType.SubCommand)
                .AddOption("query", ApplicationCommandOptionType.String, "The search query", isRequired: true, isAutocomplete: false)
            );
        PlexBot.client.ButtonExecuted += ButtonExecuted;

        PlexBot.client.SlashCommandExecuted += async Task (command) =>
        {
            switch (command.Data.Name)
            {
                case "tv":
                    await Handle(command);
                    break;
            }
        };
        await PlexBot.client.CreateGlobalApplicationCommandAsync(tvCommand.Build());
    }

    private static async Task ButtonExecuted(SocketMessageComponent component)
    {
        switch (component.Data.CustomId)
        {
            case "watch-series-button-id":
                await component.RespondAsync($"Not yet implemented! {component.User.Mention}!");
                break;

            case "add-series-button-id":
                if (SearchResult.ContainsKey(component.User))
                {
                    SeriesModel series = SearchResult[component.User];
                    if (SonarrController.Add(series))
                    {
                        EmbedBuilder embedBuilder = new EmbedBuilder()
                            .WithTitle($"{component.User.Username} added {series.Title}")
                            .WithImageUrl(series.Images.Poster.ToString())
                            .WithColor(Color.Green)
                            .WithCurrentTimestamp()
                            .WithDescription(series.Overview);
                        await component.RespondAsync(embed: embedBuilder.Build(), ephemeral: false);
                    }
                    else
                    {
                        await component.RespondAsync($"Unable to add series: {series.Title}", ephemeral: true);
                    }
                    SearchResult.Remove(component.User);
                }
                else
                {
                    await component.RespondAsync($"No request found for user: {component.User.Mention}", ephemeral: true);
                }
                break;
        }
    }
}