// LFInteractive LLC. - All Rights Reserved
using Chase.Plex_Bot.Core.Controller;
using Chase.Plex_Bot.Core.Model;
using Chase.Plex_Bot.SabNzbd.Model;
using Newtonsoft.Json.Linq;

namespace Chase.Plex_Bot.SabNzbd.Controller;

public static class SabController
{
    public static DownloadQueueModel GetQueue()
    {
        DownloadQueueModel model = new();
        using (HttpClient client = new())
        {
            using HttpRequestMessage request = GetRequest("queue");
            using HttpResponseMessage response = client.Send(request);
            if (response.IsSuccessStatusCode)
            {
                JObject json = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                json = json.GetValue("queue")?.ToObject<JObject>() ?? new JObject();
                model.Paused = json.GetValue("paused")?.ToObject<bool>() ?? false;
                model.TotalBytes = (long)(double.Parse(json.GetValue("mb")?.ToObject<string>() ?? "0.0") * 1000 * 1000);
                model.RemainingBytes = (long)(double.Parse(json.GetValue("mbleft")?.ToObject<string>() ?? "0.0") * 1000 * 1000);
                model.BytesPerSecond = (long)(double.Parse(json.GetValue("kbpersec")?.ToObject<string>() ?? "0.0") * 1000);
                model.TimeRemaining = json.GetValue("timeleft")?.ToObject<TimeSpan>() ?? TimeSpan.Zero;
                JArray array = json.GetValue("slots")?.ToObject<JArray>() ?? new JArray();
                List<DownloadModel> list = new();
                foreach (JObject slot in array)
                {
                    list.Add(new()
                    {
                        Name = slot.GetValue("filename")?.ToObject<string>() ?? "",
                        Category = slot.GetValue("cat")?.ToObject<string>() ?? "",
                        Status = slot.GetValue("status")?.ToObject<string>() ?? "",
                        Size = (long)(double.Parse(slot.GetValue("mb")?.ToObject<string>() ?? "0.0") * 1000 * 1000),
                        BytesRemaining = (long)(double.Parse(slot.GetValue("mbleft")?.ToObject<string>() ?? "0.0") * 1000 * 1000),
                        Percentage = float.Parse(slot.GetValue("percentage")?.ToObject<string>() ?? "0.0")
                    });
                }
                model.Items = list.ToArray();
            }
        }
        return model;
    }

    private static HttpRequestMessage GetRequest(string mode)
    {
        ConfigModel.CoreConfigModel config = ConfigController.Instance.Get.SabNZBD;
        return new()
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri($"{config.Host}:{config.Port}/api?apikey={config.Token}&output=json&mode={mode}")
        };
    }
}