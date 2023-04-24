// LFInteractive LLC. - All Rights Reserved
using Chase.Plex_Bot.Core.Controller;
using Chase.Plex_Bot.Core.Model;
using Chase.Plex_Bot.Sonarr.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;

namespace Chase.Plex_Bot.Sonarr.Controller;

public static class RadarrController
{
    public static bool Add(MovieModel model)
    {
        ConfigModel.ArrConfigModel config = ConfigController.Instance.Get.Radarr;
        string json = JsonConvert.SerializeObject(new
        {
            config.QualityProfileId,
            tmdbId = model.ID,
            config.RootFolderPath,
            model.Title,
            Path = Path.Combine(config.RootFolderPath, $"{model.Title} ({model.Year})"),
            config.LanguageProfileId,
            Monitored = true,
            searchForMovie = true
        });
        using HttpClient client = new();
        using HttpRequestMessage request = GetRequest($"/movie");
        request.Method = HttpMethod.Post;
        request.Content = new StringContent(json, null, "application/json");
        using HttpResponseMessage response = client.SendAsync(request).Result;
        return response.IsSuccessStatusCode;
    }

    public static void GetRootFolder()
    {
        if (!string.IsNullOrWhiteSpace(ConfigController.Instance.Get.Radarr.RootFolderPath))
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
                string root = JObject.FromObject(array[0]).GetValue("path")?.ToObject<string>() ?? ConfigController.Instance.Get.Radarr.RootFolderPath;
                ConfigController.Instance.Get = new()
                {
                    Radarr = new()
                    {
                        Host = ConfigController.Instance.Get.Radarr.Host,
                        Token = ConfigController.Instance.Get.Radarr.Token,
                        Port = ConfigController.Instance.Get.Radarr.Port,
                        RootFolderPath = root,
                        LanguageProfileId = ConfigController.Instance.Get.Radarr.LanguageProfileId,
                        QualityProfileId = ConfigController.Instance.Get.Radarr.QualityProfileId
                    },
                    Plex = ConfigController.Instance.Get.Plex,
                    SabNZBD = ConfigController.Instance.Get.SabNZBD,
                    Sonarr = ConfigController.Instance.Get.Sonarr
                };

                Log.Debug("Setting radarr root folder as {ROOT DIRECTORY}", root);
            }
        }
    }

    public static string? GetTrailerURL(int id)
    {
        using HttpClient client = new();
        using HttpResponseMessage response = client.GetAsync($" https://api.themoviedb.org/3/movie/{id}/videos?api_key={ConfigController.Instance.Get.TMDbToken}&language=en-US").Result;
        if (response.IsSuccessStatusCode)
        {
            JArray json = JArray.FromObject(JObject.Parse(response.Content.ReadAsStringAsync().Result)["results"]);
            foreach (JObject obj in json.Cast<JObject>())
            {
                if (obj.GetValue("type")?.ToObject<string>()?.Equals("Trailer") ?? false)
                {
                    string? key = obj.GetValue("key")?.ToString();
                    if (key != null)
                    {
                        return $"https://youtu.be/{key}";
                    }
                }
            }
            foreach (JObject obj in json.Cast<JObject>())
            {
                string? key = obj.GetValue("key")?.ToString();
                if (key != null)
                {
                    return $"https://youtu.be/{key}";
                }
            }
        }
        Log.Error("Unable to get trailer for: {id}", id);
        return null;
    }

    public static void Init()
    {
        Log.Information("Initializing Radarr");
        GetRootFolder();
        ConfigController.Instance.Save();
    }

    public static MovieModel[] Search(string query)
    {
        List<MovieModel> model = new();
        using (HttpClient client = new())
        {
            using HttpRequestMessage request = GetRequest($"/movie/lookup?term={query}");
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
                    int tmdb = item.GetValue("tmdbId")?.ToObject<int>() ?? 0;
                    model.Add(new()
                    {
                        Title = item.GetValue("title")?.ToObject<string>() ?? "",
                        Studio = item.GetValue("studio")?.ToObject<string>() ?? "",
                        Overview = item.GetValue("overview")?.ToObject<string>() ?? "",
                        Monitored = item.GetValue("monitored")?.ToObject<bool>() ?? false,
                        Year = item.GetValue("year")?.ToObject<int>() ?? 0,
                        Images = imageModel,
                        Added = item.ContainsKey("path"),
                        ID = tmdb,
                        Trailer = GetTrailerURL(tmdb)
                    });
                }
            }
        }
        return model.ToArray();
    }

    private static HttpRequestMessage GetRequest(string mode)
    {
        ConfigModel.ArrConfigModel config = ConfigController.Instance.Get.Radarr;

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