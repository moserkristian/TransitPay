using TicketMicroservice.Domain.Common.BaseEntities;

namespace TicketMicroservice.Domain.Common.Interfaces;

public interface IRepository<TAggregate>
    where TAggregate : AggregateRoot
{

}
