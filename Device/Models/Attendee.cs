using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Device.Models;

public class Attendee
{
    [JsonProperty("id")]
    public string Id { get; set; }
    [JsonProperty("braceletId")]
    public string BraceletId { get; set; }
    [JsonProperty("eventId")]
    public string EventId { get; set; }
    [JsonProperty("name")]
    public string Name { get; set; }
    [Range(0, 120)]
    [JsonProperty("age")]
    public int Age { get; set; }
    [JsonProperty("image")]
    public string Image { get; set; }
    [JsonProperty("arrival")]
    public DateTime Arrival { get; set; }
    [JsonProperty("departure")]
    public DateTime Departure { get; set; }

    public string ToJson()
    {
        return JsonConvert.SerializeObject(this);
    }

    public override string ToString()
    {
        return $"Attendee: {Name} ({Age}) with bracelet {BraceletId} at {Arrival} to {Departure}";
    }
}