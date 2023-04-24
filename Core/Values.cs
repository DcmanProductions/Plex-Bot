// LFInteractive LLC. - All Rights Reserved
namespace Chase.Plex_Bot.Core;

public static class Values
{
    public static class Directories
    {
        public static string Config => Directory.CreateDirectory(Path.Combine(Root, "config")).FullName;
        public static string Logs => Directory.CreateDirectory(Path.Combine(Root, "logs")).FullName;
        public static string Root => Directory.CreateDirectory(Path.GetFullPath("./data")).FullName;
    }

    public static class Files
    {
        public static string Settings => Path.Combine(Directories.Config, "settings.json");
    }
}