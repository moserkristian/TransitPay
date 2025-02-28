namespace TicketMicroservice.Application.Common.Messaging;

public interface IMessagePublisher
{
    Task PublishMessageAsync(string message);
}
