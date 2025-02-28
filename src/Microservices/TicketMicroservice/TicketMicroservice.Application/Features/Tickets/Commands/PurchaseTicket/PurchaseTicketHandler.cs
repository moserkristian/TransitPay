using MediatR;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using TicketMicroservice.Application.Common.Messaging;
using TicketMicroservice.Domain.Aggregates.TicketAggregate;

namespace TicketMicroservice.Application.Features.Tickets.Commands.PurchaseTicket;

public class PurchaseTicketHandler(
    ITicketRepository ticketRepository,
    IMessagePublisher messagePublisher,
    ILogger<PurchaseTicketHandler> logger)
    : IRequestHandler<PurchaseTicketCommand, PurchaseTicketResult>
{
    public async Task<PurchaseTicketResult> Handle(
        PurchaseTicketCommand request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating ticket: {@Request}", request);

        // Business logic for creating a ticket
        var ticket = new Ticket();
        await ticketRepository.AddAsync(ticket, cancellationToken);

        logger.LogInformation("Successfully created ticket for {EventName}", request);

        var message = JsonSerializer.Serialize(new
        {
            id = "ticket.Id",
            ticketNumber = "ticket.TicketNumber",
            amount = "ticket.Amount"
        });

        await messagePublisher.PublishMessageAsync(message);

        return new PurchaseTicketResult
        {
        };
    }
}