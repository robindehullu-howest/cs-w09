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
using System.Linq;
using System.Collections.Generic;

namespace Server.Triggers;

public class GetAttendeesByEventIdTrigger
{
    [FunctionName("GetAttendeesByEventIdTrigger")]
    public async Task<IActionResult> GetAttendeesByEventId(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "event/{eventid}/attendees")] HttpRequest req, string eventId,
        ILogger log)
    {
        if (!string.IsNullOrEmpty(eventId))
        {
            Container container = GetCosmosDBContainer("cloud services", "Attendees");

            QueryDefinition queryDefinition = new QueryDefinition("SELECT * FROM c");
            var iterator = container.GetItemQueryIterator<Attendee>(queryDefinition, requestOptions: new QueryRequestOptions() { PartitionKey = new PartitionKey(eventId) });
            List<Attendee> attendees = new();
            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                foreach (var attendee in response)
                {
                    attendees.Add(attendee);
                }
            }

            return new OkObjectResult(attendees);
        }

        return new BadRequestObjectResult("No attendees found.");
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

