// LFInteractive LLC. - All Rights Reserved
namespace Chase.Plex_Bot.Sonarr.Model;

public struct SeriesModel
{
    public bool Added { get; set; }
    public DateTime AddedDate { get; set; }
    public bool Ended { get; set; }
    public int ID { get; set; }
    public ImageModel Images { get; set; }
    public bool Monitored { get; set; }
    public string Network { get; set; }
    public string Overview { get; set; }
    public int Seasons { get; set; }
    public string Title { get; set; }
    public int Year { get; set; }
}