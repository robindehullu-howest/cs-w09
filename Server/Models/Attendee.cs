using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Server.Models;

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

    public override string ToString()
    {
        return JsonConvert.SerializeObject(this);
    }
}