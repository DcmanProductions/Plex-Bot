// LFInteractive LLC. - All Rights Reserved
namespace Chase.Plex_Bot.Core.Model;

public struct ConfigModel
{
    public string DiscordToken { get; set; }
    public CoreConfigModel Plex { get; set; }
    public ArrConfigModel Radarr { get; set; }
    public CoreConfigModel SabNZBD { get; set; }
    public ArrConfigModel Sonarr { get; set; }
    public string TMDbToken { get; set; }

    public struct ArrConfigModel
    {
        public string Host { get; set; }
        public int LanguageProfileId { get; set; }
        public int Port { get; set; }
        public int QualityProfileId { get; set; }
        public string RootFolderPath { get; set; }
        public string Token { get; set; }
    }

    public struct CoreConfigModel
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string Token { get; set; }
    }
}