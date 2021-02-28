using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace TeamsEventBot
{
    public static class Event
    {
        [FunctionName("Event")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string validationToken = req.Query["validationToken"];
            if (!string.IsNullOrEmpty(validationToken))
            {
                // Because validationToken is a string, OkObjectResult
                // will return a text/plain response body, which is
                // required for validation
                return new OkObjectResult(validationToken);
            }

            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            log.LogInformation($"Change notification payload: {requestBody}");

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            // Deserialize the JSON payload into a list of ChangeNotification
            // objects
            var notifications = JsonSerializer.Deserialize<ChangeNotificationList>(requestBody, jsonOptions);

            _ = ProcessNotificationList(notifications, log);

            return new AcceptedResult();
        }
        private static async Task ProcessNotificationList(ChangeNotificationList notificationList, ILogger log)
        {
            string clientState = System.Environment.GetEnvironmentVariable("ClientState", EnvironmentVariableTarget.Process);
            foreach (var notification in notificationList.Value)
            {
                if (clientState == notification.ClientState)
                {
                    log.LogInformation($"{notification.Resource} was {notification.ChangeType}");
                }
                else
                {
                    log.LogError($"Notification received with unexpected client state: {notification.ClientState}");
                }
            }
        }
    }

    // Represents a change notification payload
    // https://docs.microsoft.com/graph/api/resources/changenotification?view=graph-rest-1.0
    public class ChangeNotification
    {
        public string ChangeType { get; set; }
        public string ClientState { get; set; }
        public string Resource { get; set; }
        public ResourceData ResourceData { get; set; }
        public DateTime SubscriptionExpirationDateTime { get; set; }
        public string SubscriptionId { get; set; }
        public string TenantId { get; set; }
    }

    // Class to represent the resourceData object
    // inside a change notification
    public class ResourceData
    {
        public string Id { get; set; }
    }

    // Class representing an array of notifications
    // in a notification payload
    public class ChangeNotificationList
    {
        public ChangeNotification[] Value { get; set; }
    }
}
