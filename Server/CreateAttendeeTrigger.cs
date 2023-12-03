using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Cosmos;

namespace Server.Triggers;
public class CreateAttendeeTrigger
{
    [FunctionName("CreateAttendeeTrigger")]
    public async Task<IActionResult> CreateAttendee(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "attendee")] HttpRequest req,
        ILogger log)
    {
        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        Attendee attendeeData = JsonConvert.DeserializeObject<Attendee>(requestBody);
        attendeeData.Id = Guid.NewGuid().ToString();
        attendeeData.BraceletId = Guid.NewGuid().ToString();
        attendeeData.Arrival = DateTime.Now;

        Container container = GetCosmosDBContainer("cloud services", "Attendees");
        attendeeData = await container.CreateItemAsync(attendeeData, new PartitionKey(attendeeData.EventId));

        return new OkObjectResult(attendeeData);
    }

    // Function for Cosmos DB connection and container
    private Container GetCosmosDBContainer(string databaseName, string containerName)
    {
        var connectionString = Environment.GetEnvironmentVariable("CosmosDBConnectionString");
        CosmosClientOptions options = new CosmosClientOptions() { ConnectionMode = ConnectionMode.Gateway };
        var cosmosClient = new CosmosClient(connectionString, options);

        var database = cosmosClient.GetDatabase(databaseName);
        var container = database.GetContainer(containerName);

        return container;
    }
}

