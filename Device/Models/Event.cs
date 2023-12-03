using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Device.Models;

public class Event
{
    [JsonProperty("id")]
    public string Id { get; set; }
    [JsonProperty("name")]
    public string Name { get; set; }
    [JsonProperty("city")]
    public string City { get; set; }
    [JsonProperty("createdBy")]
    [EmailAddress]
    public string CreatedBy { get; set; }
    [JsonProperty("start")]
    public DateTime Start { get; set; }
    [JsonProperty("stop")]
    public DateTime Stop { get; set; }

    public string ToJson()
    {
        return JsonConvert.SerializeObject(this);
    }

    public override string ToString()
    {
        return $"Event: {Name} in {City} created by {CreatedBy} on {Start} to {Stop}";
    }
}
