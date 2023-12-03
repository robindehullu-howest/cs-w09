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

public class GetAttendeeByBraceletIdTrigger
{
    [FunctionName("GetAttendeeByBraceletIdTrigger")]
    public async Task<IActionResult> GetAttendeeByBraceletId(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "attendee/bracelet/{id}")] HttpRequest req, string id,
        ILogger log)
    {
        if (!string.IsNullOrEmpty(id))
        {
            Container container = GetCosmosDBContainer("cloud services", "Attendees");

            var query = new QueryDefinition($"SELECT * FROM c WHERE c.braceletId = @braceletId")
                    .WithParameter("@braceletId", id);

            var iterator = container.GetItemQueryIterator<dynamic>(query);

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                Attendee attendeeData = JsonConvert.DeserializeObject<Attendee>(response.FirstOrDefault().ToString());

                if (attendeeData != null)
                    return new OkObjectResult(attendeeData);
            }
        }

        return new BadRequestObjectResult("Attendee not found.");
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

