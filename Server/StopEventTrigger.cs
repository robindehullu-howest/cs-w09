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

namespace Server.Triggers;

public class StopEventTrigger
{
    [FunctionName("StopEventTrigger")]
    public async Task<IActionResult> StopEvent(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "event/{id}/stop")] HttpRequest req, string id,
        ILogger log)
    {
        if (!string.IsNullOrEmpty(id))
        {
            Container container = GetCosmosDBContainer("cloud services", "Events");

            var query = new QueryDefinition($"SELECT * FROM c WHERE c.id = @id")
                    .WithParameter("@id", id);

            var iterator = container.GetItemQueryIterator<dynamic>(query);

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                Event eventData = JsonConvert.DeserializeObject<Event>(response.FirstOrDefault().ToString());

                if (eventData != null && eventData.Start != DateTime.MinValue && eventData.Stop == DateTime.MinValue)
                {
                    eventData.Stop = DateTime.Now;

                    eventData = await container.ReplaceItemAsync(eventData, eventData.Id.ToString(), new PartitionKey(eventData.City));

                    return new OkObjectResult(eventData);
                }
                else
                    return new BadRequestObjectResult("Event already stopped.");
            }
        }

        return new BadRequestObjectResult("No event found. Please create a new event.");
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

