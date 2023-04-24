// LFInteractive LLC. - All Rights Reserved
namespace Chase.Plex_Bot.SabNzbd.Model;

public enum DownloadStatus
{
}

public struct DownloadModel
{
    public long BytesDownloaded => Size - BytesRemaining;
    public long BytesRemaining { get; set; }
    public string Category { get; set; }
    public string Name { get; set; }

    public float Percentage { get; set; }
    public long Size { get; set; }
    public string Status { get; set; }
}