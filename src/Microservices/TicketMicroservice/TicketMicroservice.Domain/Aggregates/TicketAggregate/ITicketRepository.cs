using TicketMicroservice.Domain.Common.Interfaces;

namespace TicketMicroservice.Domain.Aggregates.TicketAggregate;

public interface ITicketRepository
    : IRepository<Ticket>
{
    Task<Ticket> AddAsync(Ticket entity, CancellationToken cancellationToken);
}
