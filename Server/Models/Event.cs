using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Server.Models;

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

    public override string ToString()
    {
        return JsonConvert.SerializeObject(this);
    }
}
