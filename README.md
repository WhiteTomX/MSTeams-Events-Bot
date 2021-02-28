# Teams-Event-Bot
Get notifications about new and changed Events directly in Teams!

Currently you can create Events (Meetings) in the Calender App directly in Teams. Unfortunately Teams does not notify the invited atendees. It justs creates a normal invitation, that you can view in Outlook. But who whants to use Outlook?
Teams Event Bot to the rescue! Everytime a Event is added to your calender (because you were invited) it will message you abot it!

## Architecture
The bot uses serverless Azure Functions to avoid costs during idle time. The bot only notifies users without interaction, so it's a notification only bot.
![Architecture](Architecture.svg)

### Management Function
This is a time triggered function. It will first check Table storage for the number of Eventsubscription we should have running. This number is compared to the actual [subscriptions](https://docs.microsoft.com/en-us/graph/api/resources/subscription?view=graph-rest-1.0). If there is a missmatch, we delete the [deltaLink ](https://docs.microsoft.com/en-us/graph/api/user-delta?view=graph-rest-1.0&tabs=http)from TableStorage

Next we will get the [deltaLink ](https://docs.microsoft.com/en-us/graph/api/user-delta?view=graph-rest-1.0&tabs=http)from Table storage. If there is no delta Link we need to process all users instead. For each user we check if there is an valid subscription (correct Url). If not we create the subscription.

### /Event Function
This is the HTTPS Endpoint to which the Graph API will send change Notification about events. We just relay the information to the user here.
