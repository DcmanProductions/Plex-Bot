// LFInteractive LLC. - All Rights Reserved
using Chase.Bot;
using Chase.Plex_Bot.Sonarr.Controller;
using Chase.Plex_Bot.Sonarr.Model;
using Discord;
using Discord.WebSocket;
using Serilog;

namespace Chase.Plex_Bot.Bot.Commands;

internal static class SlashMovieCommand
{
    public static Dictionary<SocketUser, MovieModel> SearchResult = new();

    public static async Task Handle(SocketSlashCommand command)
    {
        string method = command.Data.Options.First().Name;
        string query = command.Data.Options.First().Options.First().Value.ToString() ?? "";
        switch (method)
        {
            case "search":
                Log.Information("{USER} is searching for movie {movie}", command.User.Username, query);
                MovieModel[] results = RadarrController.Search(query);
                if (!results.Any())
                {
                    await command.RespondAsync($"No Movie found with title of \"{query}\"", ephemeral: true);

                    break;
                }
                MovieModel result = results[0];
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
                var component = new ComponentBuilder();
                if (!result.Added)
                {
                    component.WithButton($"Add", "add-movie-button-id", ButtonStyle.Primary);
                }
                else
                {
                    component.WithButton($"Watch Together", "watch-movie-button-id", ButtonStyle.Primary);
                }
                if (result.Trailer != null)
                {
                    component.WithButton($"Watch Trailer", "watch-movie-trailer-button-id", ButtonStyle.Secondary);
                }
                await command.RespondAsync(embed: embedBuilder.Build(), components: component.Build(), ephemeral: true);
                break;
        }
    }

    public static async Task Register()
    {
        SlashCommandBuilder movieCommand = new SlashCommandBuilder()
            .WithName("movie")
            .WithDescription("Handles Movie Series")
            .AddOptions(new SlashCommandOptionBuilder()
                .WithName("search")
                .WithDescription("Searches for Movies")
                .WithType(ApplicationCommandOptionType.SubCommand)
                .AddOption("query", ApplicationCommandOptionType.String, "The search query", isRequired: true, isAutocomplete: false)
            );
        PlexBot.client.ButtonExecuted += ButtonExecuted;

        PlexBot.client.SlashCommandExecuted += async Task (command) =>
        {
            switch (command.Data.Name)
            {
                case "movie":
                    await Handle(command);
                    break;
            }
        };
        await PlexBot.client.CreateGlobalApplicationCommandAsync(movieCommand.Build());
    }

    private static async Task ButtonExecuted(SocketMessageComponent component)
    {
        switch (component.Data.CustomId)
        {
            case "add-movie-button-id":
                if (SearchResult.ContainsKey(component.User))
                {
                    MovieModel movie = SearchResult[component.User];
                    if (RadarrController.Add(movie))
                    {
                        EmbedBuilder embedBuilder = new EmbedBuilder()
                            .WithTitle($"{component.User.Username} added {movie.Title}")
                            .WithImageUrl(movie.Images.Poster.ToString())
                            .WithColor(Color.Green)
                            .WithCurrentTimestamp()
                            .WithDescription(movie.Overview);
                        await component.RespondAsync(embed: embedBuilder.Build(), ephemeral: false);
                    }
                    else
                    {
                        await component.RespondAsync($"Unable to add movie: {movie.Title}", ephemeral: true);
                    }
                    SearchResult.Remove(component.User);
                }
                else
                {
                    await component.RespondAsync($"No request found for user: {component.User.Mention}", ephemeral: true);
                }
                break;

            case "watch-movie-trailer-button-id":
                if (SearchResult.ContainsKey(component.User))
                {
                    MovieModel movie = SearchResult[component.User];
                    await component.RespondAsync(movie.Trailer, ephemeral: true);
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