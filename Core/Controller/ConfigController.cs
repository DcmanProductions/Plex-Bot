// LFInteractive LLC. - All Rights Reserved
using Chase.Plex_Bot.Core.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;

namespace Chase.Plex_Bot.Core.Controller;

public class ConfigController
{
    public static ConfigController Instance = Instance ??= new();

    protected ConfigController()
    {
        Get = new ConfigModel()
        {
            DiscordToken = "",
            TMDbToken = "",
            Plex = new()
            {
                Host = "",
                Port = 32400,
                Token = ""
            },
            SabNZBD = new()
            {
                Host = "",
                Port = 8080,
                Token = ""
            },
            Radarr = new()
            {
                Host = "",
                Port = 7878,
                Token = "",
                RootFolderPath = "",
                LanguageProfileId = 1,
                QualityProfileId = 1
            },
            Sonarr = new()
            {
                Host = "",
                Port = 8989,
                Token = "",
                RootFolderPath = "",
                LanguageProfileId = 1,
                QualityProfileId = 1
            }
        };
        Load();
    }

    public ConfigModel Get { get; set; }

    public void Load()
    {
        if (!File.Exists(Values.Files.Settings))
        {
            Save();
        }
        using FileStream fs = new(Values.Files.Settings, FileMode.Open, FileAccess.Read, FileShare.Read);
        using StreamReader reader = new(fs);
        Get = JObject.Parse(reader.ReadToEnd()).ToObject<ConfigModel>();

        Log.Debug("Loading Config:\n{CONFIG}", JsonConvert.SerializeObject(Get, Formatting.Indented));
    }

    public void Save()
    {
        using FileStream fs = new(Values.Files.Settings, FileMode.Create, FileAccess.Write, FileShare.None);
        using StreamWriter writer = new(fs);
        writer.Write(JsonConvert.SerializeObject(Get, Formatting.Indented));
        Log.Debug("Saving Config:\n{CONFIG}", JsonConvert.SerializeObject(Get, Formatting.Indented));
    }

    public bool Validate()
    {
        bool valid = true;
        if (string.IsNullOrWhiteSpace(ConfigController.Instance.Get.DiscordToken))
        {
            Log.Error("Discord Token is EMPTY");
            valid = false;
        }
        if (string.IsNullOrWhiteSpace(ConfigController.Instance.Get.TMDbToken))
        {
            Log.Error("TMDb Token is EMPTY");
            valid = false;
        }
        if (string.IsNullOrWhiteSpace(ConfigController.Instance.Get.Radarr.Token))
        {
            Log.Error("Radarr Token is EMPTY");
            valid = false;
        }
        if (string.IsNullOrWhiteSpace(ConfigController.Instance.Get.Sonarr.Token))
        {
            Log.Error("Sonarr Token is EMPTY");
            valid = false;
        }
        if (string.IsNullOrWhiteSpace(ConfigController.Instance.Get.SabNZBD.Token))
        {
            Log.Error("SabNZBD Token is EMPTY");
            valid = false;
        }
        if (string.IsNullOrWhiteSpace(ConfigController.Instance.Get.Plex.Token))
        {
            Log.Error("Plex Token is EMPTY");
            valid = false;
        }
        if (string.IsNullOrWhiteSpace(ConfigController.Instance.Get.Radarr.Host))
        {
            Log.Error("Radarr Host is EMPTY");
            valid = false;
        }
        if (string.IsNullOrWhiteSpace(ConfigController.Instance.Get.Sonarr.Host))
        {
            Log.Error("Sonarr Host is EMPTY");
            valid = false;
        }
        if (string.IsNullOrWhiteSpace(ConfigController.Instance.Get.SabNZBD.Host))
        {
            Log.Error("SabNZBD Host is EMPTY");
            valid = false;
        }
        if (string.IsNullOrWhiteSpace(ConfigController.Instance.Get.Plex.Host))
        {
            Log.Error("Plex Host is EMPTY");
            valid = false;
        }
        return valid;
    }
}