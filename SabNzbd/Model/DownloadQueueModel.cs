// LFInteractive LLC. - All Rights Reserved
namespace Chase.Plex_Bot.SabNzbd.Model;

public struct DownloadQueueModel
{
    public long BytesPerSecond { get; set; }
    public long DownloadedBytes => TotalBytes - RemainingBytes;
    public DownloadModel[] Items { get; set; }
    public bool Paused { get; set; }
    public long RemainingBytes { get; set; }
    public TimeSpan TimeRemaining { get; set; }
    public long TotalBytes { get; set; }
}