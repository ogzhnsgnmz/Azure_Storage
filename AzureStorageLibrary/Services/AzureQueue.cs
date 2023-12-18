using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;

namespace AzureStorageLibrary.Services;

public class AzureQueue
{
    private readonly QueueClient _queueClient;

    public AzureQueue(string queueName)
    {
        var connectionString = "DefaultEndpointsProtocol=https;AccountName=projectstorage0;AccountKey=8OV1y2vOFMzEZjZwANhU8Sz+uY99odgdfTWOXyYDQnE+njqXv5xPXMYWKvOVL3IcJ1zos1Tjnbc0+ASt0t8vpA==;EndpointSuffix=core.windows.net";
        _queueClient = new QueueClient(connectionString, queueName);
        //_queueClient = new QueueClient(ConnectionStrings.AzureStorageConnectionString, queueName);
        _queueClient.CreateIfNotExists();
    }

    public async Task SendMessageAsync(string message)
    {
        await _queueClient.SendMessageAsync(message);
    }

    public async Task<QueueMessage?> RetrieveNextMessageAsync()
    {
        QueueProperties properties = await _queueClient.GetPropertiesAsync();

        //_queueClient.PeekMessageAsync();
        if (properties.ApproximateMessagesCount>0)
        {
            QueueMessage[] queueMessages = await _queueClient.ReceiveMessagesAsync(1, TimeSpan.FromMinutes(1));
            
            if (queueMessages.Any())
                return queueMessages[0];
        }
        return null;
    }

    public async Task DeleteMessage(string messageId, string popReceipt)
    {
        await _queueClient.DeleteMessageAsync(messageId, popReceipt);
    }
}
