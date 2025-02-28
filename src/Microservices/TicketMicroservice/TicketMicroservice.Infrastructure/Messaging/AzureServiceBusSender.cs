using Azure.Messaging.ServiceBus;
using TicketMicroservice.Application.Common.Messaging;

namespace TicketMicroservice.Infrastructure.Messaging;

public class AzureServiceBusSender
    : IMessagePublisher
{
    private readonly ServiceBusSender _sender;

    public AzureServiceBusSender(ServiceBusClient client, string queueName)
    {
        _sender = client.CreateSender(queueName);
    }

    public async Task PublishMessageAsync(string message)
    {
        await _sender.SendMessageAsync(new ServiceBusMessage(message));
    }
}
