# TransitPay
Zúčtovanie lístkov vo verejnej doprave
Integrácia s bankovými systémami
Propojenie verejnej dopravy s platobnými technológiami

# Microservices Architecture
TransitPay.sln
├── src
│   ├── Microservices
│   │   ├── TicketMicroservice
│   │   │   ├── TicketMicroservice.Api
│   │   │   ├── TicketMicroservice.Application
│   │   │   ├── TicketMicroservice.Domain
│   │   │   └── TicketMicroservice.Infrastructure
│   │   ├── TransactionMicroservice
│   │   │   ├── TransactionMicroservice.Api
│   │   │   ├── TransactionMicroservice.Application
│   │   │   ├── TransactionMicroservice.Domain
│   │   │   └── TransactionMicroservice.Infrastructure
│   │   └── SettlementMicroservice
│   │       ├── SettlementMicroservice.Api
│   │       ├── SettlementMicroservice.Application
│   │       ├── SettlementMicroservice.Domain
│   │       └── SettlementMicroservice.Infrastructure
│   ├── Presentation
│   │   ├── TransitPay.Web
│   │   ├── TransitPay.Admin
│   │   └── TransitPay.Gateway
│   └── Shared
│       ├── TransitPay.ServiceDefaults
│       ├── TransitPay.AppHost
│       ├── TransitPay.SharedKernel
│       └── TransitPay.Common
└── tests
    ├── TicketMicroservice.Tests
    ├── TransactionMicroservice.Tests
    ├── SettlementMicroservice.Tests
    ├── TransitPay.IntegrationTests
    └── TransitPay.PerformanceTests

# TicketService
Spracováva nákup a validáciu lístkov
Zabezpečuje generovanie QR kódov / NFC tokenov pre validáciu
Prijíma platby (volá TransactionService)
Komunikácia:
REST API pre web a mobilnú aplikáciu
Event-driven messaging s TransactionService (RabbitMQ alebo Azure Service Bus)

# TransactionService
Spracováva platby od zákazníkov
Integruje sa s bankovými bránami (Stripe, Adyen, ČSOB API)
Ukladá informácie o transakciách
Komunikácia:
REST API pre TicketService
Publikuje eventy pre SettlementService

# SettlementService
Raz mesačne vykonáva zúčtovanie medzi dopravcami
Počíta provízie a rozdeľuje platby
Generuje finančné reporty pre dopravcov
Komunikácia:
Prijíma transakčné dáta z TransactionService
Generuje exporty pre účetné oddelenia

# Clean Architecture
TicketService
├── TicketService.Api
│   ├── Controllers
│   ├── Middlewares
│   ├── DTOs
│   ├── Program.cs
│   └── Startup.cs
├── TicketService.Application
│   ├── Interfaces
│   ├── Services
│   ├── Commands
│   ├── Queries
│   └── Validators
├── TicketService.Domain
│   ├── Entities
│   ├── Events
│   ├── ValueObjects
│   ├── Interfaces
│   ├── Enums
│   └── Exceptions
└── TicketService.Infrastructure
    ├── Persistence
    ├── Repositories
    ├── ExternalServices
    ├── Messaging
    └── Configurations

Popis vrstiev:
API: Poskytuje HTTP/REST rozhranie pre komunikáciu s frontend aplikáciou.
Application: Obsahuje business logiku (CQRS, Validácie, Služby).
Domain: Definuje doménové entity a pravidlá.
Infrastructure: Implementuje perzistenciu a integrácie s externými systémami.

API referencuje Application & Infrastructure.
Infrastructure referencuje Application.
Application referencuje Domain.

# Integrácia s bankovými systémami a finančnými platformami
Integráciu s bankami a pokladňami riešime v TransactionService nasledovne:

API pre prijímanie platieb:

Endpoint: POST /api/transactions/pay
Volané frontend aplikáciou
Messaging event pre SettlementService:

transaction.completed – Odoslané po úspešnej platbe
transaction.failed – Odoslané v prípade neúspechu
Integrácia s platobnými bránami:

Implementácia IPaymentGateway rozhrania pre Stripe, ČSOB API atď.

# Komunikácia medzi mikroservismi
Použijeme Event-Driven Architecture cez Azure Service Bus.

TicketService → TransactionService:
ticket.purchased event pri kúpe lístka
TransactionService → SettlementService:
transaction.completed event pri úspešnej platbe