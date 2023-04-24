// LFInteractive LLC. - All Rights Reserved
using Chase.Plex_Bot.Core.Controller;
using Chase.Plex_Bot.Core.Model;
using Chase.Plex_Bot.Sonarr.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;

namespace Chase.Plex_Bot.Sonarr.Controller;

public static class SonarrController
{
    public static bool Add(SeriesModel model)
    {
        ConfigModel.ArrConfigModel config = ConfigController.Instance.Get.Sonarr;
        string json = JsonConvert.SerializeObject(new
        {
            config.QualityProfileId,
            TvdbId = model.ID,
            config.RootFolderPath,
            model.Title,
            Path = Path.Combine(config.RootFolderPath, $"{model.Title} ({model.Year})"),
            config.LanguageProfileId,
            Monitored = true,
            addOptions = new
            {
                monitor = "all",
                searchForMissingEpisodes = true,
                SearchForCutoffUnmetEpisodes = true
            }
        });
        using HttpClient client = new();
        using HttpRequestMessage request = GetRequest($"/series");
        request.Method = HttpMethod.Post;
        request.Content = new StringContent(json, null, "application/json");
        using HttpResponseMessage response = client.SendAsync(request).Result;
        return response.IsSuccessStatusCode;
    }

    public static void GetRootFolder()
    {
        if (!string.IsNullOrWhiteSpace(ConfigController.Instance.Get.Sonarr.RootFolderPath))
        {
            return;
        }
        using HttpClient client = new();
        using HttpRequestMessage request = GetRequest($"/rootfolder");
        using HttpResponseMessage response = client.SendAsync(request).Result;
        if (response.IsSuccessStatusCode)
        {
            JArray array = JArray.Parse(response.Content.ReadAsStringAsync().Result);
            if (array.Any())
            {
                string root = JObject.FromObject(array[0]).GetValue("path")?.ToObject<string>() ?? ConfigController.Instance.Get.Sonarr.RootFolderPath;
                ConfigController.Instance.Get = new()
                {
                    Radarr = ConfigController.Instance.Get.Radarr,
                    Plex = ConfigController.Instance.Get.Plex,
                    SabNZBD = ConfigController.Instance.Get.SabNZBD,
                    Sonarr = new()
                    {
                        Host = ConfigController.Instance.Get.Sonarr.Host,
                        Token = ConfigController.Instance.Get.Sonarr.Token,
                        Port = ConfigController.Instance.Get.Sonarr.Port,
                        RootFolderPath = root,
                        LanguageProfileId = ConfigController.Instance.Get.Sonarr.LanguageProfileId,
                        QualityProfileId = ConfigController.Instance.Get.Sonarr.QualityProfileId
                    }
                };

                Log.Debug("Setting sonarr root folder as {ROOT DIRECTORY}", root);
            }
        }
    }

    public static void Init()
    {
        Log.Information("Initializing Sonarr");
        GetRootFolder();
        ConfigController.Instance.Save();
    }

    public static SeriesModel[] Search(string query)
    {
        List<SeriesModel> model = new();
        using (HttpClient client = new())
        {
            using HttpRequestMessage request = GetRequest($"/series/lookup?term={query}");
            using HttpResponseMessage response = client.SendAsync(request).Result;
            if (response.IsSuccessStatusCode)
            {
                JArray array = JArray.Parse(response.Content.ReadAsStringAsync().Result);
                foreach (JObject item in array.Cast<JObject>())
                {
                    ImageModel imageModel = new();
                    JArray images = item.GetValue("images")?.ToObject<JArray>() ?? new();
                    foreach (JObject image in images)
                    {
                        switch (image.GetValue("coverType")?.ToObject<string>())
                        {
                            case "banner":
                                imageModel.Banner = image.GetValue("remoteUrl")?.ToObject<Uri>() ?? new("");
                                break;

                            case "poster":
                                imageModel.Poster = image.GetValue("remoteUrl")?.ToObject<Uri>() ?? new("");
                                break;

                            case "fanart":
                                imageModel.Fanart = image.GetValue("remoteUrl")?.ToObject<Uri>() ?? new("");
                                break;
                        }
                    }
                    model.Add(new()
                    {
                        Title = item.GetValue("title")?.ToObject<string>() ?? "",
                        Network = item.GetValue("network")?.ToObject<string>() ?? "",
                        Overview = item.GetValue("overview")?.ToObject<string>() ?? "",
                        Ended = item.GetValue("ended")?.ToObject<bool>() ?? false,
                        AddedDate = item.GetValue("added")?.ToObject<DateTime>() ?? DateTime.Parse("0001-01-01T00:00:00Z"),
                        Added = item.ContainsKey("path"),
                        Monitored = item.GetValue("monitored")?.ToObject<bool>() ?? false,
                        Seasons = item.GetValue("statistics")?.ToObject<JObject>()?.GetValue("seasonCount")?.ToObject<int>() ?? 0,
                        Year = item.GetValue("year")?.ToObject<int>() ?? 0,
                        Images = imageModel,
                        ID = item.GetValue("tvdbId")?.ToObject<int>() ?? 0
                    });
                }
            }
        }
        return model.ToArray();
    }

    private static HttpRequestMessage GetRequest(string mode)
    {
        ConfigModel.ArrConfigModel config = ConfigController.Instance.Get.Sonarr;

        return new()
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri($"{config.Host}:{config.Port}/api/v3{mode}"),
            Headers = {
                { "x-api-key", config.Token }
            },
        };
    }
}