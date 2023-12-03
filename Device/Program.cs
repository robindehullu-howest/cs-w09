
using System.Security.Cryptography;
using System.Text;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using Newtonsoft.Json;

string connectionString = "HostName=lab07-iothub.azure-devices.net;DeviceId=eventclient;SharedAccessKey=8WDHGWbQnrd6qmV7eEHwv+OX53m5OPHEdAIoTK858js=";

var deviceClient = DeviceClient.CreateFromConnectionString(connectionString);
await deviceClient.SetDesiredPropertyUpdateCallbackAsync(OnDesiredPropertyChanged, null);

string eventId = await GetEventIdAsync();

while (true)
{
    Console.WriteLine();
    Console.WriteLine("[EventClient]");
    Console.WriteLine("1. Create event");
    Console.WriteLine("2. Get event");
    Console.WriteLine("3. Start event");
    Console.WriteLine("4. Stop event");
    Console.WriteLine("5. Create attendee");
    Console.WriteLine("6. Get attendee by id");
    Console.WriteLine("7. Get attendee by bracelet id");
    Console.WriteLine("8. Get attendees");
    Console.WriteLine("9. Exit");

    Console.WriteLine();
    Console.WriteLine("Choose an option:");
    int option = int.Parse(Console.ReadLine());
    Console.WriteLine();

    switch (option)
    {
        case 1:
            Console.WriteLine("Enter event name:");
            string eventName = Console.ReadLine();

            Console.WriteLine("Enter event city:");
            string city = Console.ReadLine();

            Console.WriteLine("Enter email of event creator:");
            string email = Console.ReadLine();

            Console.WriteLine();
            Console.WriteLine("Creating event...");
            Console.WriteLine();

            await CreateEventAsync(eventName, city, email);
            break;
        case 2:
            await GetEventAsync(eventId);
            break;
        case 3:
            await StartEventAsync(eventId);
            break;
        case 4:
            await StopEventAsync(eventId);
            break;
        case 5:
            Console.WriteLine("Enter attendee name:");
            string attendeeName = Console.ReadLine();

            Console.WriteLine("Enter attendee age:");
            int age = int.Parse(Console.ReadLine());

            await CreateAttendeeAsync(eventId, attendeeName, age);
            break;
        case 6:
            Console.WriteLine("Enter attendee id:");
            string id = Console.ReadLine();

            await GetAttendeeByIdAsync(id);
            break;
        case 7:
            Console.WriteLine("Enter attendee bracelet id:");
            string braceletId = Console.ReadLine();

            await GetAttendeeByBraceletIdAsync(braceletId);
            break;
        case 8:
            await GetAttendeesAsync(eventId);
            break;
        case 9:
            Console.WriteLine("Exiting...");
            return;
        default:
            Console.WriteLine("Invalid option");
            break;
    }

    Console.WriteLine();
    Console.WriteLine("Press any key to continue...");
    Console.ReadKey();
}


Task OnDesiredPropertyChanged(TwinCollection desiredProperties, object userContext)
{
    if (desiredProperties.Contains("event"))
    {
        eventId = desiredProperties["event"];
        Console.WriteLine($"Event changed: {eventId}");
        Console.WriteLine();
    }
    return Task.CompletedTask;
}

async Task<string> GetEventIdAsync()
{
    var twin = await deviceClient.GetTwinAsync();
    if (twin.Properties.Desired.Contains("event"))
        return twin.Properties.Desired["event"];
    else
    {
        Console.WriteLine("No event id found");
        return null;
    }
}

async Task CreateEventAsync(string name, string city, string email)
{
    Event eventData = new Event
    {
        Name = name,
        City = city,
        CreatedBy = email
    };

    using (HttpClient client = new HttpClient())
    {
        StringContent content = new StringContent(eventData.ToJson(), Encoding.UTF8, "application/json");

        HttpResponseMessage response = await client.PostAsync($"http://localhost:7071/api/event", content);

        if (response.IsSuccessStatusCode)
        {
            string responseBody = await response.Content.ReadAsStringAsync();
            eventData = JsonConvert.DeserializeObject<Event>(responseBody);
            Console.WriteLine(eventData);
        }
        else Console.WriteLine("Event not created");
    }
}

async Task GetEventAsync(string eventId)
{
    using (HttpClient client = new HttpClient())
    {
        HttpResponseMessage response = await client.GetAsync($"http://localhost:7071/api/event/{eventId}");

        if (response.IsSuccessStatusCode)
        {
            string responseBody = await response.Content.ReadAsStringAsync();
            Event eventData = JsonConvert.DeserializeObject<Event>(responseBody);
            Console.WriteLine(eventData);
        }
        else Console.WriteLine("Event not found");
    }
}

async Task StartEventAsync(string eventId)
{
    using (HttpClient client = new HttpClient())
    {
        HttpResponseMessage response = await client.PutAsync($"http://localhost:7071/api/event/{eventId}/start", null);
        string content = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine("Event started:");
            Event eventData = JsonConvert.DeserializeObject<Event>(content);
            Console.WriteLine(eventData);
        }
        else
        {
            Console.WriteLine("Event not started:");
            Console.WriteLine(content);
        }
    }
}

async Task StopEventAsync(string eventId)
{
    using (HttpClient client = new HttpClient())
    {
        HttpResponseMessage response = await client.PutAsync($"http://localhost:7071/api/event/{eventId}/stop", null);
        string content = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine("Event stopped:");
            Event eventData = JsonConvert.DeserializeObject<Event>(content);
            Console.WriteLine(eventData);
        }
        else
        {
            Console.WriteLine("Event not stopped:");
            Console.WriteLine(content);
        }
    }
}

async Task CreateAttendeeAsync(string eventId, string name, int age)
{
    Attendee attendee = new Attendee
    {
        EventId = eventId,
        Name = name,
        Age = age,
        Image = GenerateGravatarUrl(GenerateEmail(name))
    };

    using (HttpClient client = new HttpClient())
    {
        StringContent body = new StringContent(attendee.ToJson(), Encoding.UTF8, "application/json");

        HttpResponseMessage response = await client.PostAsync($"http://localhost:7071/api/attendee", body);
        string content = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine("Attendee created:");
            attendee = JsonConvert.DeserializeObject<Attendee>(content);
            Console.WriteLine(attendee);
        }
        else
        {
            Console.WriteLine("Attendee not created:");
            Console.WriteLine(content);
        }
    }
}

string GenerateEmail(string name)
{
    string replacedName = name.Replace(' ', '.');
    string domain = "@example.com";
    return replacedName + domain;
}

string GenerateGravatarUrl(string email)
{
    using (SHA256 sha256Hash = SHA256.Create())
    {
        byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(email));

        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < bytes.Length; i++)
        {
            builder.Append(bytes[i].ToString("x2"));
        }

        return "https://www.gravatar.com/avatar/" + builder.ToString();
    }
}

async Task GetAttendeeByIdAsync(string id)
{
    using (HttpClient client = new HttpClient())
    {
        HttpResponseMessage response = await client.GetAsync($"http://localhost:7071/api/attendee/{id}");

        if (response.IsSuccessStatusCode)
        {
            string responseBody = await response.Content.ReadAsStringAsync();
            Attendee attendee = JsonConvert.DeserializeObject<Attendee>(responseBody);
            Console.WriteLine(attendee);
        }
        else Console.WriteLine("Attendee not found");
    }
}

async Task GetAttendeeByBraceletIdAsync(string id)
{
    using (HttpClient client = new HttpClient())
    {
        HttpResponseMessage response = await client.GetAsync($"http://localhost:7071/api/attendee/bracelet/{id}");

        if (response.IsSuccessStatusCode)
        {
            string responseBody = await response.Content.ReadAsStringAsync();
            Attendee attendee = JsonConvert.DeserializeObject<Attendee>(responseBody);
            Console.WriteLine(attendee);
        }
        else Console.WriteLine("Attendee not found");
    }
}

async Task GetAttendeesAsync(string eventId)
{
    using (HttpClient client = new HttpClient())
    {
        HttpResponseMessage response = await client.GetAsync($"http://localhost:7071/api/event/{eventId}/attendees");

        if (response.IsSuccessStatusCode)
        {
            string responseBody = await response.Content.ReadAsStringAsync();
            List<Attendee> attendees = JsonConvert.DeserializeObject<List<Attendee>>(responseBody);
            foreach (var attendee in attendees)
            {
                Console.WriteLine(attendee);
            }
        }
        else Console.WriteLine("Attendees not found");
    }
}