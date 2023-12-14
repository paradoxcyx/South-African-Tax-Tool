using Newtonsoft.Json;

namespace southafricantaxtool.SARSScraper.Models;

public class ImportantDate
{
    [JsonProperty("title")]
    public string Title { get; set; }

    [JsonProperty("description")]
    public string Description { get; set; }

    [JsonProperty("start")]
    public DateTimeOffset Start { get; set; }

    [JsonProperty("end")]
    public DateTimeOffset End { get; set; }

    [JsonProperty("url")]
    public string Url { get; set; }

    [JsonProperty("allDay")]
    public string AllDay { get; set; }
}