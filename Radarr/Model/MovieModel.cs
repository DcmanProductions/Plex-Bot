// LFInteractive LLC. - All Rights Reserved
namespace Chase.Plex_Bot.Sonarr.Model;

public struct MovieModel
{
    public bool Added { get; set; }
    public int ID { get; set; }
    public ImageModel Images { get; set; }
    public bool Monitored { get; set; }
    public string Overview { get; set; }
    public string Studio { get; set; }
    public string Title { get; set; }
    public string? Trailer { get; set; }
    public int Year { get; set; }
}