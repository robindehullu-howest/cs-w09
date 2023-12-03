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
using Microsoft.Azure.Devices;

namespace Server.Triggers;

public class CreateEventTrigger
{
    [FunctionName("CreateEventTrigger")]
    public async Task<IActionResult> CreateEvent(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "event")] HttpRequest req,
        ILogger log)
    {
        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        Event eventData = JsonConvert.DeserializeObject<Event>(requestBody);
        eventData.Id = Guid.NewGuid().ToString();

        Container container = GetCosmosDBContainer("cloud services", "Events");
        eventData = await container.CreateItemAsync(eventData, new PartitionKey(eventData.City));

        RegistryManager registryManager = GetIoTHubRegistryManager();
        IQuery devices = registryManager.CreateQuery("SELECT * FROM devices WHERE STARTSWITH(deviceId, 'eventclient')");
        while (devices.HasMoreResults)
        {
            var page = await devices.GetNextAsTwinAsync();
            foreach (var twin in page)
            {
                twin.Properties.Desired["event"] = eventData.Id;
                await registryManager.UpdateTwinAsync(twin.DeviceId, twin, twin.ETag);
            }
        }

        return new OkObjectResult(eventData);
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

    // Function for IoT Hub connection
    private RegistryManager GetIoTHubRegistryManager()
    {
        return RegistryManager.CreateFromConnectionString(Environment.GetEnvironmentVariable("IotHubAdminConnectionString"));
    }
}


