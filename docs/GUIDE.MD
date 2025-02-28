Ticketing Settlement System – Architecture & Implementation Guide
This guide outlines a fully-featured ticketing settlement system built with a Clean Architecture approach and a microservices design. We integrate with banking systems, ticket-selling devices, and financial platforms, using .NET 8 + .NET Aspire for orchestration. The frontend is implemented in Blazor with Radzen components, SQL Server is used as the primary database, and Azure Service Bus enables messaging between services. We also leverage Azure AD for secure authentication and authorization. The system is designed for Azure deployment, ensuring scalability and maintainability.
Overview of Architecture and Technologies
Clean Architecture & DDD: Each microservice is built with Clean Architecture principles (separating Domain, Application, Infrastructure, and Presentation layers) to enforce a clear separation of concerns​.
This ensures core business logic (Domain) is isolated from external concerns like UI or database, improving modularity and testability. Each service focuses on its bounded context (e.g., ticketing, payments) and encapsulates its own domain models and logic​.
Microservices Approach: The system is decomposed into multiple self-contained services (e.g., Ticketing Service, Settlement/Payment Service, etc.), each running independently. Services communicate asynchronously via events on Azure Service Bus to decouple interactions. This event-driven approach improves resiliency – if one service is down, others can continue and eventually catch up when it recovers, achieving eventual consistency​.
.NET Aspire Orchestration: .NET Aspire is used to orchestrate the microservices during development. Aspire simplifies running and configuring multiple services together by providing an AppHost (for service orchestration and a dashboard) and ServiceDefaults (common cross-cutting setups)​.
It handles service discovery, configuration of shared resources (like Service Bus, SQL, Key Vault, etc.), and streamlines local development of distributed systems.
Frontend (Blazor + Radzen): The user interface is implemented as a Blazor application (either Blazor Server or WebAssembly) utilizing Radzen UI components for rapid UI development and a rich user experience. This frontend communicates with the backend microservices via secure HTTP APIs. Radzen provides data grids, forms, and charts that accelerate building the ticket management UI, and it natively supports Azure AD authentication integration​.
SQL Server Database: Each microservice uses SQL Server for persistent storage of its data. Following microservice isolation, each service can have its own database or schema to ensure loose coupling (e.g., Ticketing DB, Settlement DB). Using Azure SQL Database in the cloud ensures scalability and managed service benefits. .NET Entity Framework Core is used in the Infrastructure layer of each service for data access (with migrations for schema management).
Azure Service Bus Messaging: Azure Service Bus acts as the message broker for the system's asynchronous communication. It enables reliable messaging between microservices – for example, when a ticket is sold, the Ticketing Service publishes an event to a Service Bus topic/queue that the Settlement Service consumes. This decoupling via a messaging channel is a key integration logic for a resilient, scalable system​.
Authentication & Authorization (Azure AD): The system uses Azure Active Directory for securing user access. The Blazor frontend is registered as an Azure AD application for user sign-in. Users authenticate with Azure AD (OAuth 2.0/OpenID Connect), and the obtained JWT access tokens are used to authorize calls to the microservice APIs. Azure AD ensures robust security and easy role management. Radzen’s security mechanisms can be configured to use Azure AD, so users can log in with their Azure credentials​.
Each microservice API will validate tokens from Azure AD (using JWT Bearer authentication middleware) and enforce role-based access control for sensitive operations.
With this high-level overview in mind, we now proceed step-by-step through the implementation, from setting up the solution structure to deploying on Azure.
Step 1: Solution Structure and Project Setup
Organize the codebase into a solution with multiple projects reflecting the Clean Architecture layers and microservices. A recommended structure is:
sql
Copy code
/TicketingSettlementSystem.sln        -- Visual Studio solution (or use `dotnet` CLI to manage).
 src/
  ├── TicketingService/               -- Ticketing microservice bounded context
  │    ├── Ticketing.Domain/          -- Domain layer (enterprise logic for ticketing)
  │    ├── Ticketing.Application/     -- Application layer (use cases, service interfaces)
  │    ├── Ticketing.Infrastructure/  -- Infrastructure layer (EF Core, external integrations)
  │    └── Ticketing.API/             -- Presentation layer (Web API controllers for ticketing)
  |
  ├── SettlementService/              -- Settlement/Payment microservice
  │    ├── Settlement.Domain/         -- Domain layer (payment & settlement core logic)
  │    ├── Settlement.Application/    -- Application layer (payment processing use cases)
  │    ├── Settlement.Infrastructure/ -- Infrastructure (EF Core, banking API clients)
  │    └── Settlement.API/            -- Presentation (Web API controllers for settlement)
  |
  ├── [Other Services]/              -- (Optional) Additional microservices for other contexts 
  │    └── ...                        -- e.g., UserManagement, Reporting, etc., following same layer structure
  |
  ├── Frontend/                      
  │    └── TicketingSettlement.BlazorApp/ -- Blazor front-end (with Radzen components)
  │         ├── Pages/               -- Blazor pages (UI screens for ticketing, payments, etc.)
  │         ├── Shared/              -- Shared UI components or layouts
  │         └── Program.cs, Startup.cs (if Blazor Server) or Program.cs (if Blazor WASM) 
  |
  ├── BuildingBlocks/                -- (Optional) common libraries (if needed for shared types, e.g., cross-cutting concerns or messaging contracts)
  │    ├── TicketingSettlement.Contracts/ -- Definitions of integration events, DTOs shared between services
  │    └── TicketingSettlement.ServiceDefaults/ -- .NET Aspire ServiceDefaults project (common config)
  |
  ├── TicketingSettlement.AppHost/   -- .NET Aspire AppHost project for orchestration
  └── TicketingSettlement.sln        -- Solution file (references all projects)
Explanation:
Each microservice (Ticketing, Settlement, etc.) is implemented as a grouping of four projects corresponding to Clean Architecture layers. This separation ensures that business logic is decoupled from frameworks. For example, the Domain project has entities like Ticket, Order, Payment, plus domain services or logic with no external dependencies​.
The Application project contains use case classes, commands/queries, and interfaces (e.g., ITicketRepository, IPaymentGateway) that define what the app needs from outside without implementing details​.
Infrastructure implements these interfaces (e.g., using EF Core for data access, or an API client for banking) but depends on Application for the interface definitions​.
The API project is the web layer (controllers or Razor pages) that depends on Application to execute use cases and on Infrastructure for DI registration – but the core logic remains in Application/Domain. This enforces the rule that outer layers depend on inner layers, never the reverse​.
The Blazor frontend is a separate project under Frontend/. We use Radzen’s component library in the Blazor app to create pages for managing tickets, viewing settlements, etc. Radzen Studio (if used) can generate some of this structure, but even without the Radzen IDE, we can install the Radzen Blazor components via NuGet and use them in our Blazor pages. The front-end will call the backend microservices via HTTP APIs (secured with Azure AD). If using Blazor WebAssembly, the calls go directly from the browser; if Blazor Server, calls can be server-side using HttpClient. In both cases, the APIs endpoints (e.g., https://<server>/ticketing/api/...) should be accessible.
The BuildingBlocks section is optional. It can include a project for contracts or shared DTOs/events that multiple services use. For instance, define an Integration Events library that contains message formats (e.g., a TicketSoldEvent class) so that both the publisher and subscriber use the same definition. This avoids duplication and ensures consistency in message contracts. Also, a ServiceDefaults project is added by .NET Aspire tooling – this contains pre-configured setups for things like logging, metrics, health checks, etc., that can be imported by services to avoid repetition​.
The TicketingSettlement.AppHost project is generated by .NET Aspire. It acts as an orchestrator to run the whole system. Think of it as a lightweight local orchestrator: it can start multiple microservice projects together, manage their configuration (like injecting the correct connection strings, service URLs, etc.), and provide a dashboard for monitoring services in development​.
The AppHost will reference each microservice’s API project to include them in the orchestration.
Setting up the Solution:
Create Projects: Use the .NET CLI or Visual Studio to create class libraries for Domain, Application, Infrastructure, and an ASP.NET Web API project for each microservice. For example:
bash
Copy code
dotnet new classlib -n Ticketing.Domain  
dotnet new classlib -n Ticketing.Application  
dotnet new classlib -n Ticketing.Infrastructure  
dotnet new webapi   -n Ticketing.API
Repeat for Settlement, etc. Ensure each microservice’s projects reference appropriately (Domain <– Application <– Infrastructure, and API references Application & Infrastructure). Enforce no direct reference from Domain to Infrastructure (to preserve Clean Arch boundaries).
Add .NET Aspire: In Visual Studio, you can add Aspire support. For example, right-click the solution or one of the projects and choose "Add .NET Aspire Orchestrator Support" (or use the CLI with dotnet aspire). This will add two projects: AppHost (orchestrator) and ServiceDefaults​.
In the AppHost’s startup (typically Program.cs in AppHost), register each microservice project:
csharp
Copy code
builder.AddProject<Projects.Ticketing.API>("ticketing");
builder.AddProject<Projects.Settlement.API>("settlement");
// Add others similarly
This configuration tells Aspire about our services. Aspire provides service discovery so that services can find each other by name (e.g., the Blazor app or one API can locate another by the name we registered). It also centralizes configuration like connection strings in one place (often in AppHost’s appsettings). After adding, you’ll have an Aspire dashboard to run all services together in development, which is extremely handy.
Configure Project References: In each microservice’s API project, reference its corresponding Infrastructure, Application (and Domain if needed). The Infrastructure should reference Domain & Application to implement their interfaces. Use dependency injection in the API’s Startup (or Program) to register implementations from Infrastructure (e.g., EF Core DbContext, repository classes, Azure Service Bus clients, etc.). Because of Clean Architecture, your Application layer might define an interface like ITicketRepository and Infrastructure provides TicketRepository : ITicketRepository. In the API Startup, you’ll do services.AddScoped<ITicketRepository, TicketRepository>(); to inject it. This way, Application (and Domain) remain independent of the actual data access technology.
Shared Packages: Add necessary NuGet packages to each layer:
Domain: likely minimal or none (maybe package for domain events or validation if needed).
Application: perhaps MediatR (if implementing CQRS with mediator pattern for use cases), FluentValidation for validations, etc.
Infrastructure: EntityFrameworkCore (SQL Server provider), Azure.Messaging.ServiceBus (for interacting with Service Bus), HttpClient/Refit or similar (for calling external bank APIs if needed), etc.
API: AspNetCore packages (Controllers, Swagger for API docs, Azure AD JWT auth, etc.), and reference Radzen.Blazor for the Blazor project.
Folder Organization: Within each project, organize further by feature or aggregate. For example, in Ticketing.Domain, you might have Entities folder (Ticket, Event, Customer, etc.), ValueObjects, DomainEvents. In Ticketing.Application, you may have UseCases or Services (e.g., SellTicketCommandHandler, TicketQueryService), DTOs, and Interfaces (like IEventRepository, IPaymentService if Ticketing needs to call payment). Use a consistent layout to make the solution easy to navigate.
By structuring the solution in this way, each microservice is cleanly separated, and the overall solution remains organized. New team members can find domain logic isolated (in Domain/Application) and infrastructure concerns (EF, messaging) in the Infrastructure project, which aligns with best practices for maintainability​.
Step 2: Implementing Microservices with Clean Architecture
With the projects in place, implement each microservice’s functionality following Clean Architecture patterns:
2.1 Ticketing Service (Microservice 1)
Responsibilities: This service manages ticket lifecycle – e.g., creating tickets (or trips/passes if it's transit), processing ticket sales from devices or the frontend, and recording ticket usage. It integrates with ticket-selling devices (like kiosks or handheld devices) by exposing APIs those devices call to sell or validate tickets. Key features could include event listing (if for events), ticket inventory, and issuing tickets to customers.Domain Layer (Ticketing.Domain): Define core entities and value objects, for example:
Ticket entity (with properties like TicketId, EventId/RouteId, Price, Status, etc.).
SaleTransaction entity (if tracking each ticket sale, maybe containing TicketId, Timestamp, Amount, PaymentStatus).
Perhaps Device entity if managing devices, or treat devices as external with just an ID that comes in requests.
Domain events: e.g., TicketSoldEvent (to trigger downstream processing). A domain event can be published when a ticket sale is completed in the domain layer. This could later be translated into an integration event for Service Bus in the application layer.
Ensure business rules are enforced in the domain models (e.g., a Ticket cannot be marked as sold more than once, etc.). Keep these rules within methods on the entities or domain services.Application Layer (Ticketing.Application): Implement use cases as services or command handlers. For example:
Commands: SellTicketCommand (with handler logic to deduct availability, mark ticket as sold, create a sale transaction, etc.), RefundTicketCommand, etc.
Queries: GetAvailableTicketsQuery, GetTicketStatusQuery, etc. These return DTOs or read models for the frontend.
Interfaces: Define ITicketRepository for ticket data access, IDeviceService if the application needs to verify device identity, and an event publisher interface like IMessagePublisher for publishing integration events (abstracting the Service Bus so application layer just calls an interface when an event needs to be published).
After successfully selling a ticket (in the command handler), trigger an integration event: e.g., call IMessagePublisher.PublishTicketSold(ticketSaleDetails). This interface will be implemented in Infrastructure to actually send a message to Azure Service Bus.Infrastructure Layer (Ticketing.Infrastructure): Implement the repository and messaging:
EF Core DbContext: e.g., TicketingDbContext with DbSet<Ticket>, DbSet<SaleTransaction>, etc. Configure it for SQL Server. Use OnModelCreating to apply configurations (or use EF Fluent API classes). Use EF Migrations to create the database schema.
TicketRepository: implements ITicketRepository using EF Core to query/modify tickets.
Azure Service Bus Publisher: implement IMessagePublisher. For instance, use Azure.Messaging.ServiceBus SDK – create a ServiceBusClient (with connection string from config) and ServiceBusSender for the topic/queue (e.g., "ticketsales"). The PublishTicketSold method will serialize the event data (e.g., to JSON or binary) and send a message. Ensure to set proper message attributes (e.g., a label or message type).
Device Integration: If devices require special handling (say they connect via IoT Hub or SignalR), the Ticketing service might also have an integration component. But likely, devices just call a REST API on this service with their device ID and ticket info. We can simply treat that like any other API call – the device ID might be validated against a known list (which could be stored in this service or in Azure AD as client credentials).
Presentation Layer (Ticketing.API): Implement a RESTful API (or gRPC endpoints) to expose operations:
POST /api/tickets/sell – to sell a ticket (this calls the SellTicketCommand internally).
GET /api/tickets/{id} – retrieve ticket status.
GET /api/events/{id}/tickets – list available tickets for an event (if applicable).
etc.
Use Controllers or Minimal APIs. Ensure to secure these endpoints with authentication (Azure AD JWT) and authorization (e.g., require specific roles/claims for management operations). Also, consider that ticket-selling devices might not have user identities; they might use a client credential flow or an API key. For simplicity, you might allow a special client identity (configured in Azure AD as an App Registration with its own secret or certificate) that devices use to get a token and call these APIs.Integration Event Flow (Ticketing): When a ticket is sold via an API call, the Ticketing service will:
Validate the request (authenticate user or device, ensure ticket is available).
Use Application layer logic to record the sale and mark the ticket as sold.
Commit the transaction to the database.
Publish a TicketSold event to Azure Service Bus (to a queue/topic, e.g., ticketing/sales topic). This event includes details like TicketId, sale price, payment method or transaction id (if available), etc.
This event will be picked up by the Settlement service (and possibly others, e.g., a Reporting service if added later). By using the Service Bus, we decouple the Ticketing and Settlement services – Ticketing doesn’t need to call Settlement directly or even know if it's running. It just fires an event and continues, improving reliability and scalability​.
2.2 Settlement (Payment) Service (Microservice 2)
Responsibilities: This service handles payment processing and financial settlement. When a ticket is sold, the actual movement of money or confirmation from a bank/payment gateway is handled here. It integrates with banking systems or payment providers. It may also perform periodic settlement (e.g., end-of-day reconciliation, transferring funds to appropriate accounts).Domain Layer (Settlement.Domain): Define core entities:
Payment or Transaction entity (with fields: TransactionId, TicketId/OrderId, Amount, Status [Pending/Confirmed/Failed], PaymentMethod, Timestamp, etc.).
Possibly an Invoice or SettlementRecord entity if accumulating multiple transactions for batch settlement.
Domain logic might include validating payment amounts, handling different payment methods, etc. For example, a method ConfirmPayment() on Payment to mark it paid.
Application Layer (Settlement.Application): Use cases and interfaces:
Commands/UseCases: ProcessPaymentCommand – triggered when a new ticket sale event is received, to initiate payment processing. ConfirmPaymentCommand – to mark a payment as confirmed (maybe called after receiving callback from bank), ReconcileAccountsCommand – for end-of-day settlement.
Queries: e.g., GetPaymentStatusQuery, GetDailySettlementsQuery.
Interfaces: IPaymentRepository, IBankGateway (to call external banking API), IMessagePublisher (if Settlement also needs to publish events, e.g., PaymentConfirmed event). Possibly an IMessageConsumer interface if using an event handler pattern for incoming events (though could handle within Infrastructure via background service).
Infrastructure Layer (Settlement.Infrastructure):
EF Core DbContext: SettlementDbContext with DbSet<Payment>, etc.
PaymentRepository: Implements data access for payments.
Bank Integration: Implement IBankGateway. For example, if integrating with a bank’s API or payment gateway (like Stripe/PayPal or a banking web service), this could use HTTP calls. One might use a library or just HttpClient with the base URL and credentials for the bank API. This method would take payment info and execute the transaction (charging a card, etc.) and return a result. Since direct integration might be synchronous or asynchronous depending on the provider, design accordingly:
If synchronous (API returns immediate success/failure), you can process in one go.
If asynchronous (e.g., bank calls a webhook later), then ProcessPaymentCommand might initiate and save a Pending status, and another endpoint/consumer will handle the confirmation.
Azure Service Bus Subscriber: This service should subscribe to events from Ticketing (the TicketSold events). There are a few ways to do this:
Use Azure.Messaging.ServiceBus in a background service (hosted service) to listen on the queue/topic subscription. E.g., in API startup, register a BackgroundService that opens a processor on the Service Bus. When a message arrives, it maps it to a ProcessPaymentCommand and dispatches to the Application layer.
Alternatively, use an event bus library like MassTransit which can handle subscription and routing of messages to consumers automatically. MassTransit can subscribe to Azure Service Bus topics and call a consumer class with strongly typed message. This can simplify some boilerplate.
Ensure to handle message completion, retries, and dead-lettering in case of failures.
Message Publishing: If Settlement needs to notify other parts of the system, e.g., once payment is confirmed, it could publish a PaymentCompleted event to Service Bus (which maybe the Ticketing service or a Notification service could subscribe to, for updating ticket status or sending receipts). Implement IMessagePublisher if needed similar to Ticketing.
External Platforms: If "financial platforms" integration is needed (perhaps an accounting system or a clearinghouse), those can be additional methods or classes in Infrastructure, called at appropriate times (for instance, after daily settlement, send a summary to a financial platform via its API).
Presentation Layer (Settlement.API): Expose APIs for any direct calls:
In many cases, Settlement might largely operate on events from Ticketing. But it could also have its own API endpoints, for example:
GET /api/payments/{id} to retrieve status of a payment.
POST /api/payments to initiate a payment (if some flows require direct call).
Webhook endpoint for bank responses (e.g., POST /api/payments/confirm called by the bank gateway when a payment is completed, if the bank uses server-to-server callback).
GET /api/settlements/report?date=2025-02-26 to get settlement summary of that date, etc.
Secure these with Azure AD as well (perhaps only certain roles can query financial info).
Processing Flow (Settlement): When a TicketSold event is received via Service Bus, the Settlement service will:
Deserialize the event to get ticket sale details.
Create a new Payment record in the database with status Pending.
Call the bank/payment API (via IBankGateway). If immediate confirmation:
Update payment status to Confirmed or Failed based on result.
Publish a PaymentConfirmed event (if other services need to know, e.g., Ticketing might update ticket status to "Active" if payment succeeded, or "Canceled" if failed, or perhaps Ticketing waits for this event to finalize the ticket issuance).
Possibly trigger further actions like emailing a receipt (which could be another service or via an Azure Function).
If the payment process is asynchronous (requires waiting for a callback), then just store Pending and wait for a webhook or another message to update later.
The Settlement service can also periodically aggregate transactions and ensure money is transferred to the company's account, etc., perhaps via scheduled jobs or on-demand commands (this might be beyond this scope, but the structure allows adding such use cases in Application layer).
Integration with Ticketing: The Ticketing service might listen for PaymentConfirmed events in return. However, a simpler design is to consider the sale complete only after payment, so maybe Ticketing doesn’t issue a ticket number until payment confirmed. But if Ticketing already issued something and waiting for payment, it should handle failed payments (e.g., cancel ticket). These are business-specific details; the architecture supports either approach by exchanging events.
2.3 Additional Services (Optional)
Depending on the full scope, you might have other microservices:
User Management Service (if managing users, roles, and maybe device identities, though Azure AD covers user identities, you might not need a custom one for users).
Reporting/Analytics Service to generate reports on sales, revenue, device performance. This could subscribe to events (like TicketSold, PaymentConfirmed) and store in a data warehouse or generate real-time metrics.
Notification Service to handle emailing or texting customers (tickets, receipts) after purchase.
Device Management Service for registering and monitoring ticket-selling devices.
Each of these would follow the same Clean Architecture template. Keep them independent but integrated via the event bus or direct APIs if necessary. For instance, a Notification service could listen to PaymentConfirmed events to send a receipt email to the customer.
2.4 Best Practices Applied
Domain-Driven Design: Model the microservices after real business domains (Ticketing, Payments). Each service has a clear boundary and owns its data. Entities and value objects reflect real concepts (Ticket, Payment, etc.), making the model intuitive.
Clean Contracts: Use Data Transfer Objects (DTOs) for communication – the API layer maps domain models to DTOs to send to the client, so the front-end isn’t tightly coupled to internal models. For events, define explicit event messages with only necessary info.
Decoupling via Events: The use of Azure Service Bus ensures the Ticketing and Settlement services are loosely coupled. They operate independently and only rely on the event contracts. This asynchronous integration increases resilience: “the application is able to integrate microservices asynchronously, which helps resiliency in case of partial failures”.
Transactional Integrity: Within each service, ensure operations that need to be atomic (like saving ticket and publishing event) use proper transaction handling. Typically, one would save the data then publish the event. If using an outbox pattern, you could first save the event in DB and have a background process publish it to ensure exactly-once processing. However, a simpler approach is acceptable if occasional duplicate events are handled gracefully (idempotent consumer logic).
Validation and Error Handling: Use FluentValidation or similar in Application layer to validate inputs for use cases. Return appropriate results or throw exceptions that API can translate to correct HTTP responses (400 Bad Request for validation errors, etc.). On the event consumer side, handle failures (e.g., if bank API is down, the message can be deferred or sent to a dead-letter queue after retries, and an alert raised).
Logging and Monitoring: Use a structured logging library (Serilog, etc.) to log important actions in each service (with correlation IDs for tracing, e.g., include the ticket ID or transaction ID in logs and messages). .NET Aspire’s ServiceDefaults can set up some of this (like enabling logging and health checks uniformly)​.
Also consider using Application Insights on Azure for distributed tracing and telemetry across the microservices.
By following this structured approach, each microservice remains focused and maintainable, and the system can evolve (e.g., adding new services or changing implementations) with minimal impact on other parts​.
Step 3: Frontend Implementation (Blazor + Radzen)
The frontend is a Blazor application that provides the UI for users (e.g., administrators, ticket sellers, or customers, depending on context) to interact with the system. We incorporate Radzen components to accelerate development of rich UI features like grids, forms, and dialogs.Project Type: You can choose Blazor Server or Blazor WebAssembly:
Blazor Server: The app runs on the server, and UI updates are sent over SignalR. This keeps the .NET code on the server (easier to secure) and can directly call microservice APIs or even use EF Core if needed (though in our case, we will call APIs). It also makes Azure AD integration straightforward using server-side authentication middleware.
Blazor WebAssembly (WASM): The app runs in the browser. This offers better client performance and offline possibilities. It calls the microservice HTTP APIs directly from the browser via HttpClient. Azure AD integration uses MSAL in the browser to acquire tokens. For a complex enterprise app, Blazor WASM with an ASP.NET Core hosted backend is a common pattern, but since we already have microservices, the WASM just calls them.
For this guide, we'll assume Blazor Server for simplicity (noting differences for WASM where needed).Setting up Blazor Project: In the TicketingSettlement.BlazorApp project:
Add the Radzen.Blazor NuGet package (Radzen provides a set of UI components). In _Imports.razor, include @using Radzen and @using Radzen.Blazor. Add a Radzen <RadzenSidebar> or data grid as needed in pages.
Configure the Blazor app to use Azure AD authentication (discussed in Step 4 below).
Layout and Navigation: Create a main layout (e.g., MainLayout.razor) with a sidebar or menu for navigating between pages (Radzen has a Sidebar component you can use). Sections likely include:
Tickets (list of tickets, selling a new ticket, etc.)
Payments/Settlements (view payments status, trigger reconciliations)
Reports (if any reporting integrated)
Administration (if managing devices or other settings)
Use Blazor routing to define pages for each feature (e.g., TicketsPage.razor, PaymentsPage.razor). Each page will call the appropriate microservice via its API.Calling Microservice APIs: In a Blazor Server app, you can directly inject an HttpClient or create API client services:
Configure the HttpClient base address for each microservice. Since in Azure these might be separate URLs (or under one domain if behind API gateway), you could store them in configuration. For example, in appsettings.json of Blazor app:
json
Copy code
"ApiUrls": {
   "Ticketing": "https://mycompany-ticketing-api.azurewebsites.net",
   "Settlement": "https://mycompany-settlement-api.azurewebsites.net"
}
Then use named HttpClients: builder.Services.AddHttpClient("TicketingAPI", c => c.BaseAddress = new Uri(Configuration["ApiUrls:Ticketing"]));.
Alternatively, if you set up Azure API Management or a gateway, you might have a single endpoint and route internally, simplifying the client usage to one base URL.
Use Radzen’s services or plain Blazor to call the API. Radzen has a concept of "invoke data source" which can call REST endpoints; if using Radzen Studio, you could configure REST endpoints and Radzen will generate service methods. If coding manually, just use HttpClient or refit clients.
Displaying Data: Use Radzen components for UI:
RadzenDataGrid to display lists of tickets, payments, etc. You can bind them to collections fetched from the API.
RadzenTemplateForm or standard <EditForm> for data input (like selling a ticket or processing a payment manually).
RadzenCharts for any visualizations (e.g., sales over time).
Radzen has built-in styles and themes; ensure consistency in look and feel.
Example Workflow (Ticket Sale via UI):
User (maybe a clerk) uses the Blazor app to sell a ticket. They navigate to the "Sell Ticket" form (with fields for selecting event or route, etc.).
They submit the form. The Blazor page calls the Ticketing API’s endpoint (e.g., via HttpClient POST to /api/tickets/sell). Because the user is authenticated (with Azure AD), the request will include a Bearer token which the API will validate.
The Ticketing API processes the sale and returns a result (maybe success with a ticket ID or a failure message). The Blazor UI can show confirmation to the user (maybe display the ticket ID or a QR code).
Meanwhile, the Settlement service is handling the payment asynchronously via the event. The user might not need to wait for that in the UI (if it’s instant payment, they might see “Payment Approved” immediately; if not, perhaps the ticket is marked as pending payment).
The UI could offer a view for pending payments or allow the user to refresh to see status. The Blazor app can call the Settlement API to get the status of the payment if needed (e.g., check if Payment for that TicketId is completed).
For administration, the Blazor app might have pages like “Reconciliation” that calls Settlement service to produce a report of all sales and payments for a period.
Azure AD UI Integration: Once Azure AD is configured (next step), Radzen provides login components if needed, or you can rely on the automatic OIDC flow. Typically, on accessing the Blazor app, if not authenticated, it will redirect to Azure AD login. After login, the Blazor app has a ClaimsPrincipal for the user. You can show the user’s name, and enforce authorization in UI (e.g., [Authorize] on certain pages or showing/hiding menu items based on roles/claims).Radzen Security: Radzen’s documentation notes that it supports Azure AD authentication out-of-the-box​.
In Radzen Studio, you can configure security providers. If doing manually, it boils down to the standard ASP.NET Core authentication setup but Radzen abstracts some of it. The key is to ensure the Blazor app is registered in Azure AD and the settings (TenantId, ClientId, Domain, etc.) are in appsettings.json, then use AddAuthentication().AddAzureAD(...) or for .NET 6+ perhaps use AddMicrosoftIdentityWebApp (from Microsoft.Identity.Web). Radzen might generate code in Startup.cs for this if using its designer.By leveraging Blazor and Radzen, the frontend remains a thin client that simply calls microservice APIs and renders data. All heavy business logic is in the backend services. This keeps the front-end simple and focused on the user experience, while the backend handles the complex processes.
Step 4: Authentication and Authorization with Azure AD
Security is critical for a financial system. We use Azure Active Directory (Azure AD) for both user authentication in the frontend and securing service-to-service communication where needed.4.1 Azure AD Setup:
In the Azure portal, register an Azure AD Application for the Blazor frontend. This will give a Client (Application) ID and you’ll configure a redirect URI (for Blazor Server, e.g., https://your-app-url/signin-oidc; for Blazor WASM, you’d have a redirect to a login page or use MSAL JS).
If Blazor Server: Configure the app as a web app with Azure AD (OIDC) and set up a client secret if needed (though for OIDC in server, not strictly needed because it's user login via browser).
If Blazor WASM: Register it as a SPA in Azure AD (which enables the implicit or auth code flow with PKCE). You might need an additional app registration for an API if using scope protection (see below).
Register an Azure AD application for each API (microservice) or a single multi-scope app to represent all APIs:
Often, one registers a backend API app in Azure AD and defines scopes for the microservices. For simplicity, you can have one Azure AD app representing "TicketingSettlement API" and define scopes like Ticketing.Read, Ticketing.Write, Settlement.Read, Settlement.Process etc., or a broad API.Access. The Blazor client app would be granted permission to these scopes.
Alternatively, use Azure AD App Roles if all within one tenant and assign users roles (like "TicketingAdmin", "SettlementManager") that the app can read from tokens.
4.2 Blazor (Server) Authentication:
Use the Microsoft.Identity.Web library (if .NET 5/6+). In Program.cs (or Startup), call:
csharp
Copy code
builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = options.DefaultPolicy;
});
And ensure in appsettings.json you have AzureAd settings (Instance, Domain, TenantId, ClientId, etc., provided from the app registration).
This will set up the Blazor Server app to use Azure AD. It handles the OIDC sign-in redirect. The [Authorize] attribute can be used on Blazor components or pages to require login.
For Blazor WASM, one would use AddMsalAuthentication in Program.cs with appropriate parameters, which under the hood uses MSAL.js to log in.
4.3 Securing the APIs:
Each microservice API will be protected by Azure AD as well, so only authenticated calls are allowed:
In each API project, add the Azure AD JWT Bearer authentication. Use the AddMicrosoftIdentityWebApi (from Microsoft.Identity.Web) or the lower-level AddJwtBearer. For example, in Startup (for .NET 6, Program.cs):
csharp
Copy code
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
       .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));
builder.Services.AddAuthorization();  // add policies if needed
Here, AzureAd section would contain the TenantId, ClientId (for the API app registration), and Audience (which might be the API's client ID or URI).
Alternatively, if you used the same app registration for both client and API, you might use the same Tenant and just validate the JWT’s issuer and audience accordingly.
Use [Authorize] attribute on controllers or specific actions. If using scope or role checks, you can use [Authorize(Policy="RequireSettlementRole")] etc., and configure policies that check for certain claims (e.g., roles).
Azure AD will issue tokens for users to the client. The Blazor client then attaches the token in the Authorization header when calling the API. For Blazor Server, this can be done by acquiring token using MSAL confidential client (since server can use client credentials to get on behalf of user token, or simpler, the Blazor server can simply pass along the user’s token from the authentication session if using proxy, but typically one would call the API as the user).
A simpler approach: Since Blazor Server runs on server, the user’s cookie auth in Blazor doesn’t automatically provide a JWT for API calls. One approach is to use OAuth2 On-Behalf-Of flow: the Blazor server, when needing to call the microservice API on behalf of the user, uses its token to get an API token. Microsoft.Identity.Web can manage this via AddMicrosoftIdentityWebApp and AddDownstreamWebApi.
For Blazor WASM, MSAL handles acquiring an access token for the API scope and then you attach it to HttpClient calls easily via an authorization message handler.
For simplicity in this guide, we assume the Blazor server doesn’t directly call APIs on the server side, but rather the client calls them (which in Blazor Server means a JS call or HttpClient call from the server with user token). This can be configured, but an easier route: treat the Blazor app like a regular web app that calls APIs via JavaScript or server proxy.
4.4 Radzen Considerations:
Radzen Studio can simplify Azure AD setup. In Radzen, you’d go to the Security panel and choose Azure AD, then input the necessary IDs. Radzen would add the needed configuration under the hood. The documentation states “Radzen Blazor applications support authentication and authorization against Azure AD. Radzen application users can login via their Azure AD credentials.”​
 – meaning Radzen will handle the heavy lifting of Azure AD login flow for you.
If not using the Radzen IDE, follow the standard steps as above. The Radzen UI components will respect the authenticated state (for instance, Radzen has Login component but in Azure AD case, you might not use that since Azure AD redirect happens automatically when [Authorize] is encountered).
4.5 Protecting Service Communication:
Our microservices talk primarily via Service Bus events, which are internal and not user-driven, so they don’t need Azure AD tokens for that. The Service Bus itself is secured by connection string/keys. Ensure those connection strings are kept safe (Key Vault or environment variables).
If one microservice needed to call another’s HTTP API directly (synchronous call, which we try to minimize), then it should use an identity as well (client credentials flow with Azure AD, or an internal certificate). For example, Settlement might call a User service to get user info – it should use its own Azure AD app identity to request a token for the User API. This is advanced and for our scenario not mandatory since we use messaging.
4.6 Testing Auth Flow:
Once configured, run the Blazor app. It should redirect to Microsoft login, then back. The APIs when accessed without a token should return 401. With a valid token (from an authorized user), they should allow access. Test with different user accounts and Azure AD roles if applicable (e.g., a user without "SettlementManager" role should get 403 on a settlement report API if role-based authorization is set up).
By leveraging Azure AD, we offload user management and leverage enterprise-grade security. The system benefits from SSO, MFA (if enforced by the org), and centralized user provisioning. All sensitive actions can be tied to user identities for audit. Azure AD also simplifies granting external partners access if needed, by guest accounts or service principals, rather than managing separate credentials.
Step 5: Messaging and Workflow with Azure Service Bus
For integrating the microservices and external systems, we use Azure Service Bus as the central messaging backbone. This allows us to build an event-driven workflow: Ticket events trigger payment processing, which may trigger further events for other processes, all decoupled and scalable.5.1 Azure Service Bus Setup:
Create an Azure Service Bus Namespace (in Azure Portal or via scripts). Within the namespace, create a Topic for ticketing events (e.g., ticketing-events). We can use a single topic with subscriptions per service, or multiple queues/topics for different event types. For simplicity:
Topic: ticketing-events
Subscriptions: SettlementService-sub (Settlement service will subscribe to ticket events), and possibly others like Reporting-sub.
Alternatively, use a dedicated Queue for a one-to-one communication. E.g., a queue ticket-sold-queue that Settlement listens to. But topics allow multiple subscribers if needed.
Connection string: Note the connection string (or better, the Azure AD authentication for Service Bus using managed identity, but that’s advanced). For now, store the connection string in a secure place (local dev secrets.json, and Azure Key Vault for prod).
5.2 Integrating Service Bus in Ticketing Service:
In Ticketing.Infrastructure, after a ticket sale is saved to DB, the service needs to publish an event. Using Azure.Messaging.ServiceBus:
csharp
Copy code
public class AzureServiceBusMessagePublisher : IMessagePublisher {
    private readonly ServiceBusClient _client;
    private readonly string _topicName = "ticketing-events";
    public AzureServiceBusMessagePublisher(ServiceBusClient client) {
        _client = client;
    }
    public async Task PublishTicketSoldAsync(TicketSoldEvent evt) {
        ServiceBusSender sender = _client.CreateSender(_topicName);
        var messageBody = JsonSerializer.Serialize(evt);
        ServiceBusMessage message = new ServiceBusMessage(messageBody) {
            Subject = "TicketSold", // Label/subject to indicate type
            CorrelationId = evt.TicketId.ToString()
        };
        await sender.SendMessageAsync(message);
    }
    // ... other event publish methods
}
Here, ServiceBusClient is configured with the connection string (you can register it via DI, e.g., services.AddSingleton(new ServiceBusClient("<conn-string>"));). .NET Aspire can help inject config; for instance, if using ServiceDefaults, you might have a line like builder.AddServiceBus("<name>") in AppHost if such exists (Aspire might simplify resource connections).
Ensure this publisher is registered as IMessagePublisher in DI and the Application layer’s command handler calls it. Optionally, include error handling and retry (the SDK by default will retry transient errors).
Consider using Sessions or ordering if message order matters (e.g., if multiple events for same ticket should be processed in order, use a Session ID = TicketId).
The event payload (TicketSoldEvent) should include essential info for Settlement: at least an identifier to correlate (TicketId or SaleTransactionId), and maybe the amount to charge, payment method, user, etc. Avoid sensitive info if not needed.
5.3 Integrating Service Bus in Settlement Service:
Settlement service needs to consume the TicketSold events. There are a few patterns:
Background Service with Processor: Use ServiceBusProcessor. For example, in Settlement.API startup, do:
csharp
Copy code
services.AddSingleton<ServiceBusClient>(sp => new ServiceBusClient("<conn-string>"));
services.AddSingleton<ServiceBusProcessor>(sp => {
     var client = sp.GetRequiredService<ServiceBusClient>();
     var processor = client.CreateProcessor("ticketing-events", "SettlementService-sub");
     // Configure processor options if needed (MaxConcurrentCalls, etc.)
     processor.ProcessMessageAsync += async args => {
         string body = args.Message.Body.ToString();
         var evt = JsonSerializer.Deserialize<TicketSoldEvent>(body);
         // resolve needed service and handle event
         using(var scope = sp.CreateScope()) {
            var handler = scope.ServiceProvider.GetRequiredService<IEventHandler<TicketSoldEvent>>();
            await handler.HandleAsync(evt);
         }
         await args.CompleteMessageAsync(args.Message);
     };
     processor.ProcessErrorAsync += args => {
         // log error
         return Task.CompletedTask;
     };
     return processor;
});
services.AddHostedService<ServiceBusWorker>(); // a BackgroundService that starts the processor
This shows one way: create a single processor for the subscription; on message, deserialize and handle. The handling can be delegated to an event handler class in Application layer. You might implement IEventHandler<TicketSoldEvent> in the Application layer (similar to a command handler but for events). In ServiceBusWorker (inherits BackgroundService), in ExecuteAsync, call processor.StartProcessingAsync() and on shutdown processor.StopProcessingAsync().
Using MassTransit or NServiceBus: These libraries allow you to declare a consumer for TicketSoldEvent and they'll handle subscription, deserializing, etc. For example, MassTransit configuration would map a consumer class to the topic. This can reduce boilerplate and also integrate retries, etc. If familiarity is there, it's a good option, but to keep things straightforward, the above using raw SDK is fine.
Once the event is received, the Settlement service will perform the payment logic as described earlier (update DB, call bank API, etc.).
After processing, if needed, Settlement could publish a PaymentProcessed or PaymentConfirmed event. This would be done similarly using a ServiceBusSender on maybe another topic (or the same with different label). If Ticketing needs to consume it, Ticketing would have its own subscription or queue.
5.4 Integration with External Financial Platforms:
If the system needs to notify or update external financial platforms (say an accounting system or a revenue management platform) about transactions, use the Service Bus to decouple this as well. For example:
Settlement service after confirming payment could send an event SettlementCompleted to a finance-events topic.
An IntegrationService or Azure Function could subscribe to finance-events and then call the external platform’s API with the settlement info. This way, any hiccup in the external platform doesn’t affect the main flow; the message will retry or dead-letter if the external call fails.
Alternatively, use Azure Logic Apps or Data Factory if it’s more of a batch integration. But given the real-time nature, event-driven triggers are effective.
5.5 Testing Message Flow:
During dev, .NET Aspire will help run everything. Ensure the Service Bus connection string (likely pointing to an Azure Service Bus instance) is reachable. If offline, you could use Azure Service Bus Emulator (or switch to RabbitMQ/Local queue for dev via config if needed).
Simulate a ticket sale from the Blazor app or directly calling Ticketing API; verify Ticketing logs show event published. Check Settlement logs for receiving the event and processing payment.
Use Service Bus Explorer (Azure portal tool) to peek messages, ensure none stuck or in dead-letter. This helps troubleshoot.
Using Azure Service Bus provides a robust foundation for event-driven microservices. It ensures that messages are delivered reliably (with retries, and durable storage). The decoupling means each service can scale independently: e.g., if ticket sales spike, many events will be enqueued, and you can scale out the Settlement service (more instances reading the subscription in parallel) to process payments faster. This is a key advantage of this architecture​,
as the Service Bus acts as the backbone for distributing events and leveling load between services.
Step 6: .NET Aspire for Development Orchestration
.NET Aspire plays a crucial role in simplifying the development and testing of this microservices system. While not required for production, it greatly streamlines running the whole system locally and preparing it for cloud deployment.6.1 What .NET Aspire Provides:
Orchestration: The Aspire AppHost project we added will orchestrate multiple services. Instead of manually running each API and the Blazor app, Aspire allows one-click (or one command) to run all of them. It sets up a local environment where each service can discover others by name, and configures common dependencies (like connection strings) from a central place​.
Service Discovery: Aspire’s service discovery means our Blazor app and microservices can locate each other using a naming convention. For example, in development, Ticketing API might be reachable at http://localhost:<port> but the Blazor app can call it via an Aspire proxy or naming (depending on how Aspire implements discovery). Essentially, Aspire will update the Blazor app configuration to point to the right URI for "ticketing" service automatically.
Dashboard: When running, the AppHost often provides a dashboard (possibly a web UI on a specific port) showing the status of each service (running, stopped, logs, health checks). This makes it easy to monitor the system in dev.
Unified Configuration: Instead of duplicating certain config settings, you can put them in AppHost’s appsettings and share. For instance, the Service Bus connection string, or Azure AD tenant, could be defined once and Aspire injects into each service. (Under the hood, ServiceDefaults or AppHost might pass environment variables or use config injection).
Common Services: The ServiceDefaults library helps set up cross-cutting concerns. For example, calling builder.AddServiceDefaults(); in your microservice Program.cs (as shown in the CODE Magazine snippet) can automatically add things like:
Logging and telemetry (maybe Application Insights or console logging).
Health Checks endpoints (each service might get a /health endpoint automatically).
Perhaps integration with Key Vault, etc., if configured.
6.2 Running with .NET Aspire:
Visual Studio: After configuring the AppHost with all services (as in Step 1), set the AppHost as the startup project. Running it will launch the Aspire dashboard. The AppHost’s configuration (usually a .config/aspire.json or similar) lists which projects to launch. Alternatively, multiple startup projects can be configured, but Aspire simplifies it.
CLI: You might be able to run dotnet run in the AppHost project directory, and it will launch everything.
You should see output for each service in one place. If one crashes, Aspire might show it and allow restarting.
Access the Blazor frontend (likely at a port configured by Aspire, e.g., http://localhost:5000) and test end-to-end scenarios. Service discovery by Aspire means the Blazor app calling http://ticketing/api/... could be routed to the correct service.
6.3 Service Defaults Configurations:
You can customize the ServiceDefaults project if needed. For example, to add SQL Server DB context through Aspire: In CODE Magazine example, they show builder.AddSqlServerDbContext<BookContext>("theDb")​.
In our case, we could have:
csharp
Copy code
builder.AddSqlServerDbContext<TicketingDbContext>("TicketingConnection");
builder.AddSqlServerDbContext<SettlementDbContext>("SettlementConnection");
And in AppHost configuration, define those connection strings with keys "TicketingConnection" and "SettlementConnection". Aspire will ensure each service gets its connection string.
Similarly, if Aspire has integration for Service Bus, it might allow something like builder.AddServiceBus("<name>") or provide config for it. If not, we handle via normal config files.
6.4 Development vs Production:
It's important to note: .NET Aspire is primarily a development and testing tool. In production deployment (on Azure), we typically won't run the AppHost. Instead, each microservice will be deployed independently (e.g., as separate containers or apps). Service discovery in production might be handled by Azure infrastructure (like DNS names for services, or using an API Management). Configuration in production will come from app settings or Azure Key Vault.
However, the code we write for Aspire (like builder.AddProject<...>) is usually under a debug conditional or only in the AppHost, so it doesn’t affect production. The microservices themselves remain standard ASP.NET applications.
Aspire can also assist in containerization and generating deployment configs (e.g., maybe it can export a docker-compose or Kubernetes manifest, since it knows the components). Check Aspire docs if it has such features (it’s a new tool, evolving).
6.5 Removing Aspire for Prod:
Ensure that any development-only references (like the Projects namespace in AppHost) are not required when deploying individual services. Usually, the microservices can run independently with their own Program.cs when not under Aspire.
Aspire might inject some environment variables (like service URLs). For production, you’ll provide those via Azure app settings or config files.
Using .NET Aspire, the development team can easily run and debug the entire microservices system on their local machines, which historically is challenging with many services​.
Aspire essentially gives a lightweight alternative to container orchestration locally, letting developers focus on coding and not on Docker/K8s complexity for day-to-day work. It improves developer productivity and confidence that the integrated system works before deploying to Azure​.
Step 7: Database (SQL Server) Integration and Configuration
Each microservice uses SQL Server for data storage, so it’s important to design and configure the databases properly, ensuring isolation, consistency, and performance.7.1 Database per Service:
Following microservice principles, each service has its own database (or at least its own schema) to avoid tight coupling. This means the Ticketing service and Settlement service do not directly share tables. For example, Ticketing DB might have Tickets and Sales tables, and Settlement DB has Payments and Settlements tables.
On Azure, you could implement this as separate Azure SQL Databases for each service (e.g., TicketingDB and SettlementDB). This allows independent scaling and security boundaries. Alternatively, a single Azure SQL Server (server instance) can host both databases, which is fine – it's logically separate DBs.
The connection strings for each will differ. Store them in configuration: e.g., in AppHost’s appsettings or local secrets:
json
Copy code
"ConnectionStrings": {
  "TicketingConnection": "Server=<server>;Database=TicketingDB;User Id=...;Password=...;",
  "SettlementConnection": "Server=<server>;Database=SettlementDB;User Id=...;Password=...;"
}
Then as mentioned, either use .NET Aspire AddSqlServerDbContext or manually in each service’s Startup:
csharp
Copy code
services.AddDbContext<TicketingDbContext>(options =>
    options.UseSqlServer(Configuration.GetConnectionString("TicketingConnection")));
and similarly for Settlement (each service only needs its own connection string).
7.2 Entity Framework Core Migrations:
Use EF Core Code First Migrations to manage database schema. In each service’s Infrastructure project, add EF Core design packages so you can create migrations.
For Ticketing: run dotnet ef migrations add InitTicketingSchema -p Ticketing.Infrastructure -s Ticketing.API (using API as startup since config is there). This will create a Migrations folder in Infrastructure with the initial tables.
Do similarly for Settlement.
Migrate the databases either at startup (e.g., dbContext.Database.Migrate() in program) or via CI/CD pipeline executing dotnet ef database update. Using migrations ensures the schema is reproducible and versioned in code.
7.3 Data Models:
Design tables corresponding to your Domain models:
Ticketing: Tickets table with columns (Id, EventId, Price, Status, etc.), Sales table (SaleId, TicketId, Time, SoldBy, Price, maybe PaymentStatus if you want to mark if paid or not).
Settlement: Payments table (PaymentId, TicketId/SaleId, Amount, Status, Method, TransactionRef, Timestamp, etc.), Settlements table or DailySummary for aggregated data if needed.
Ensure primary keys and foreign keys are set appropriately (Sale references Ticket, Payment references Sale or Ticket).
Use proper data types (money/decimal for currency, etc.), and consider indexing important fields (like TicketId on Payment for quick lookup, date on Sales for reporting).
7.4 Transactions and Consistency:
Within a service, use database transactions to ensure consistency. For example, in Ticketing’s SellTicketCommandHandler, you might:
csharp
Copy code
using var txn = await _dbContext.Database.BeginTransactionAsync();
// ... create Sale, update Ticket status
await _dbContext.SaveChangesAsync();
// publish event (optionally ensure it's atomic via outbox pattern)
await txn.CommitAsync();
If publishing an event, there's a known challenge: the event might be published even if DB commit fails or vice versa. Solutions include the Outbox Pattern (save the event to DB and have a dispatcher send it after commit). Depending on requirements, you might accept a small risk or implement a basic outbox table for events. Given complexity, many systems ensure at-least-once delivery and handle duplicates to keep things simpler.
Across services, we accept eventual consistency (Ticketing and Settlement data will be consistent eventually via the event). We do not use distributed transactions across microservices (that’s an anti-pattern in microservices due to tight coupling).
7.5 Connection Management:
Using Azure SQL, be mindful of connection limits and DTUs if using Basic tiers. Use connection pooling (default in ADO). The .NET default settings are usually fine.
Each service should handle transient failures (e.g., use EnableRetryOnFailure in EF Core UseSqlServer to handle transient SQL outages).
7.6 Sensitive Data:
If storing any sensitive info (like customer payment details), ensure compliance. Possibly, for a ticketing system, we might not store full card numbers, etc. Payment service might store a token or just reference. If needed, use encryption for certain fields or rely on the payment gateway to store details.
7.7 Testing Data Layer:
Write unit tests for repositories (using in-memory or test DB) to ensure CRUD works as expected. Also integration tests could run against a local SQL (or a SQL Server container) to verify migrations and context.
7.8 Azure Cloud SQL Setup:
Create Azure SQL Database instances for each service. Alternatively, use a single Azure SQL with schemas per service. The latter could complicate independent management, so separate DBs is cleaner.
Set firewall or VNet rules so that only the app services or AKS can reach the DB. Or use managed identities for the apps to connect to Azure SQL without storing credentials (Azure SQL supports Azure AD authentication for DB).
If using managed identity: you’d give the microservice’s Azure identity access to the database (as an AAD user), and use Authentication=Active Directory Default in connection string. This avoids plain secrets.
Run the EF migrations on the Azure DB (you can set your CI/CD to run migrations, or at startup ensure it applies; in production, often DB migrations are done manually or carefully to avoid downtime).
By carefully designing the database layer and keeping each microservice’s data isolated, we ensure changes in one service’s schema don’t impact others. Scaling the database can also be done per service – e.g., if ticketing data is huge, give it a higher tier, while a smaller service can use a lower tier, optimizing costs. Clean Architecture’s separation means the majority of the system doesn’t directly depend on the database, making it feasible to swap or scale components without massive code impact.
Step 8: Azure Deployment (Infrastructure and Services)
Deploying the system to Azure involves provisioning the necessary cloud resources and configuring each component to run in the cloud environment. We aim for a setup that is scalable and manageable.8.1 Azure Resources Overview: We will need the following Azure resources for a full deployment:
App Hosting: Environment to run the microservice APIs and the Blazor frontend. Options include Azure Kubernetes Service (AKS), Azure App Service (Web Apps), Azure Container Apps, or Azure Service Fabric. Given our microservices architecture:
Azure Kubernetes Service (AKS): Ideal for containerized microservices, giving full control over networking and scaling. We can deploy each service as a Deployment in AKS and use Services or Ingress for communication.
Azure Container Apps: Simplified serverless container hosting with built-in Dapr (which overlaps somewhat with our use of Service Bus). It could be an option if we want to avoid managing K8s. It supports pub/sub with Service Bus easily.
Azure App Services: We could deploy each API as an App Service (either code or container). For small-scale systems, this is straightforward (each microservice gets its own App Service instance). The Blazor Server app could also be an App Service.
Azure Service Bus: Already discussed, ensure the namespace is created (likely in production SKU to handle expected load).
Azure SQL Database: One for each microservice (or one with multiple schemas). Create these in Azure and set up connection strings.
Azure AD: The app registrations should be done in the Azure AD tenant and configured appropriately (this is more of a configuration than a "resource" to deploy).
Azure Key Vault: Recommended to store secrets like DB connection strings, Service Bus connection key (if not using managed identity), etc. The services can load these at startup (Aspire might help, or using Azure Managed Identity to fetch from Key Vault).
Networking: If using AKS or Container Apps, set up networking such that services can communicate internally and expose publicly where needed. If using App Services, each will have a URL and you might restrict some (e.g., backend APIs not publicly accessible except via the front-end or certain clients – which could be done via VNet integration or API Management).
Azure API Management or Gateway (optional): For a cleaner approach, one can put APIM in front of the microservice APIs. APIM can handle things like aggregating the different APIs under one domain, security (validating tokens), and even transformations. If not using APIM, ensure the Blazor app knows the correct URLs.
We will illustrate using AKS as the primary method, as it's common for microservices:8.2 Containerization of Services:
Write Dockerfiles for each microservice API and for the Blazor app:
Each Dockerfile starts from mcr.microsoft.com/dotnet/aspnet:8.0 (for runtime) and use multi-stage build with mcr.microsoft.com/dotnet/sdk:8.0 to publish. For example, Ticketing.API Dockerfile:
dockerfile
Copy code
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Ticketing.API/Ticketing.API.csproj", "Ticketing.API/"]
COPY ["Ticketing.Application/Ticketing.Application.csproj", "Ticketing.Application/"]
COPY ["Ticketing.Domain/Ticketing.Domain.csproj", "Ticketing.Domain/"]
COPY ["Ticketing.Infrastructure/Ticketing.Infrastructure.csproj", "Ticketing.Infrastructure/"]
RUN dotnet restore "Ticketing.API/Ticketing.API.csproj"
COPY . .
RUN dotnet publish "Ticketing.API/Ticketing.API.csproj" -c Release -o /app/publish
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./
ENTRYPOINT ["dotnet", "Ticketing.API.dll"]
Similarly for Settlement.API. They may share some layers if done smartly (monorepo context).
Blazor Server app Dockerfile is similar (just one project to publish). If it were Blazor WASM, the client can be built and served via a static site or via an ASP.NET host.
Build and test these containers locally or using Docker. Ensure they run and can connect to a test DB and SB. (During containerization, configuration will come from environment variables, see below).
8.3 Kubernetes Deployment:
Create an AKS cluster (through Azure CLI or portal). Also create an Azure Container Registry (ACR) to push images.
Push the Docker images to ACR (or Docker Hub if not using ACR, but ACR is recommended for integration with AKS).
Write Kubernetes manifests (YAML) or use Helm charts for deploying:
One Deployment per microservice (Ticketing, Settlement, etc.), with appropriate number of replicas (start with 1, can scale to many).
Use Kubernetes Secrets to store connection strings or use pod-managed identity to access Key Vault for secrets. For initial deployment, one can mount environment variables:
yaml
Copy code
env:
  - name: ConnectionStrings__TicketingConnection
    valueFrom: secretKeyRef: ... (value: actual connection string)
  - name: AzureServiceBus__ConnectionString
    value: "Endpoint=sb://...;Key=...;"
  - name: AzureAd__TenantId
    value: "<tenant-id>"
  - name: AzureAd__ClientId
    value: "<api-client-id>"
  - name: AzureAd__IssuerUrl
    value: "https://login.microsoftonline.com/<tenant-id>/v2.0"
(Alternatively, if using Azure AD for API auth, you might not need client secret for APIs because it's JWT validation info mostly.)
Services: define a Kubernetes Service for each to allow internal communication. If we use an Ingress or API gateway, we might expose only needed ones.
For the Blazor app, if it's server, deploy similarly as a Deployment and expose it (e.g., via a LoadBalancer Service or Ingress with a URL).
Ingress: Set up an NGINX or AGIC ingress to route requests. For example:
frontend.mydomain.com -> Blazor service.
api.mydomain.com/ticketing -> Ticketing API service.
api.mydomain.com/settlement -> Settlement API. Use path-based or subdomain routing. Or use Azure API Management outside of AKS to forward to internal AKS services (APIM can have a VNet connection to AKS).
Scaling Settings: Define autoscaling if needed. For AKS, use HPA (Horizontal Pod Autoscaler) based on CPU, or even based on queue length (with KEDA for Service Bus length, if advanced). For now, note you can scale out Ticketing service if load of selling tickets is high independently of Settlement.
Monitoring: Install Azure Monitor for containers or set up Application Insights in the apps for telemetry.
8.4 Azure App Service Alternative:
If using App Services, create one App Service per API and one for the Blazor app. Deploy the code (or container) to each. Use Azure AD Easy Auth or configure authentication in the app settings for each (there is a feature to protect an app with Azure AD without changing code, but since we already did code-level auth it's not needed).
Set the App Settings for connection strings and other config in each App Service (these override appsettings.json).
Use Azure Application Gateway or simply different subdomains to access them. APIM can also be used here similarly.
8.5 Azure Container Apps Alternative:
Container Apps can be simpler for microservices: you deploy each container image as a Container App. They can all sit in the same Container Apps Environment (shared virtual network, easy internal communication). Dapr integration is built-in if enabled, which overlaps with Service Bus usage but you can still use Service Bus with Dapr pub-sub component.
Azure Container Apps will handle ingress per container app or via Dapr service invocation. This might be beyond the scope, but it's an option for future scaling without managing K8s.
8.6 CI/CD and Infrastructure as Code:
It's best practice to script the creation of these resources. Use Terraform or Azure Bicep/ARM templates:
Write templates for Service Bus, SQL databases, AKS cluster, ACR, etc. This ensures consistent environments for dev/test/prod.
For AKS, you may output kubeconfig or set up GitHub Actions to deploy manifests.
Alternatively, use the Azure CLI in pipelines to create resources. For one-off or small teams, manual creation via portal is okay to start, but infrastructure as code will pay off later.
8.7 Configuration in Azure:
Use Azure Key Vault to store secrets like DB passwords, Service Bus keys. AKS can use CSI drivers to pull secrets or apps can load from Key Vault with Managed Identities. For now, even storing in AKS Secrets is acceptable (just base64 encoded, not truly secure at rest, but limited access).
Set environment variables for each microservice as needed. Make sure to also configure any settings for Azure AD (like the authority URL, which is https://login.microsoftonline.com/{TenantId} and audience which is the API's client ID or App ID URI).
Ensure the microservices know the Service Bus connection. If the Service Bus is restricted by firewall, allow the vnet or service endpoint from AKS or App Service to access it.
Once all these are configured, you can deploy the infrastructure, then deploy the apps. At the end, you should have:
The Blazor app accessible via web (e.g., via a public URL).
The Blazor app can communicate with the APIs (internally in AKS or through a public gateway).
Azure AD protecting the front-end and the APIs.
Service Bus and SQL in the background enabling the workflows.
Test the production deployment similarly to dev: login via Azure AD on the cloud URL, try to perform a ticket sale and ensure the flow goes through (you might check logs in App Insights or containers to see the events and DB entries).
Step 9: CI/CD Pipeline Configuration
Implementing Continuous Integration and Continuous Deployment (CI/CD) is essential for a reliable and repeatable release process. We will outline a CI/CD approach using GitHub Actions (as an example; Azure DevOps or others are similar conceptually):9.1 Repository Structure for CI:
Keep all code in a single repository (monorepo) or separate repos for each microservice. A monorepo simplifies coordination but separate repos can work if teams are independent. Here, assume monorepo for simplicity.
Include a directory for deployment manifests (if using Kubernetes, e.g. /deploy/k8s/*.yaml) or Helm charts.
9.2 Continuous Integration (CI):
Trigger CI on any push or PR to the main branch.
Use a GitHub Actions workflow YAML (e.g., .github/workflows/ci.yml):
Checkout code.
Setup .NET (use actions/setup-dotnet).
Restore & Build: dotnet restore and dotnet build --configuration Release.
Run Tests: if you have unit tests, run dotnet test.
Publish Artifacts: This can produce build outputs or, more relevant, build Docker images.
Docker Build: Use Docker login to ACR (use secrets for credentials). Then docker build for each service and tag with something like myregistry.azurecr.io/ticketing-api:$(GitSha) and latest.
Push Images: docker push each image.
You can optimize by using Docker’s cache or building in parallel jobs for each service if they are truly decoupled repos.
The CI ensures that the code compiles and images are ready. It might not deploy yet (if using separate CD stage).
9.3 Continuous Deployment (CD):
Another GitHub Actions workflow (or stages in same workflow) can handle deployment. For example, on push to main or when a release is created:
If using AKS:
Use azure/login action to Azure, then azure/aks-set-context to get kubeconfig for the cluster.
Use kubectl or helm to deploy. If using raw YAML, you might do:
yaml
Copy code
kubectl set image deployment/ticketing-api ticketing-api=myregistry.azurecr.io/ticketing-api:${{ github.sha }}
to update the image to the new one (rolling update).
Or apply manifest files that use the new image tag (perhaps the manifest has image: myregistry.azurecr.io/ticketing-api:latest and you push latest tag, though using a specific tag tied to commit is better for traceability).
Run kubectl apply -f deploy/k8s if manifests are updated with the new image tag by CI (you could have a step to replace image tag in YAML before apply).
For migrations: you might have a Kubernetes Job or a step to run dotnet ef database update for each DB. Or simpler, each API on startup runs migrations (you could allow that in production if downtime is acceptable or if using zero-downtime migration techniques).
If using App Services:
Use azure/webapps-deploy GitHub action to push directly. For containerized, you'd update the App Service settings to the new image tag and maybe restart. Or if not containerized, deploy the packaged web app.
Alternatively, use Azure DevOps release pipelines with similar tasks.
If using Container Apps: use the az CLI or GH Action to deploy the container (it might auto update on new image if you configured it, or use az containerapp update command).
Environment Separation: Typically, you have separate Azure resources for dev, test, prod. Use different workflows or triggers (like merging to a dev branch deploys to dev environment). Use separate ACR repos or tags for environments or same ACR but careful promotion.
Secrets in CI/CD: Store sensitive info (Azure credentials, ACR creds) in GitHub Actions Secrets or use OIDC federation for Azure login (so GH Action can login to Azure without storing secrets, using Azure service principal with federated credentials). This is more secure and easier to manage.
Verify Deployment: After deployment, the pipeline can run integration tests. Possibly, write a small script to call an API endpoint or ping the health check of each service to verify they are running. This can alert if the deployment introduced an outage.
Rollback Strategy: Keep track of image tags and deployments to rollback if needed (for instance, in AKS you can redeploy an older image tag if something fails). Having blue-green deployment or canary is more advanced, but initial stage can be just manual rollback via previous image.
9.4 Infrastructure as Code in CI/CD:
If you have IaC (Bicep/Terraform) for Azure resources, include that in pipeline:
E.g., run az deployment group create --template-file azuredeploy.bicep to create/update infra. Or use Terraform apply. This could be separate from app deployment or integrated, depending on frequency of infra changes.
Ensure this runs with caution on prod (maybe require approval if it's altering critical resources).
9.5 Keeping Config in Sync:
Use config files or Key Vault to store settings. If using Key Vault, a step in pipeline could update KV values if needed. But more commonly, you manually set prod settings once, and pipeline doesn't change them with each deploy (unless a config change is needed).
For dev/test, pipeline could update app settings or Key Vault with new values or ensure Key Vault has required entries (like new Service Bus connection if regenerated).
9.6 Monitoring CI/CD:
Set up notifications (GitHub Action can notify on Slack/Teams/email on failure).
Protect the main branch so that PRs must pass CI before merging. This ensures that nothing goes to deployment that didn’t build and test properly.
By implementing CI/CD, the team can push changes frequently with confidence. The pipeline automates the tedious process of building, testing, and deploying each microservice. This reduces human error and speeds up delivery. Combined with infrastructure as code, it allows spinning up new environments or updating configuration in a controlled manner. The scalability of the system is also maintained through the pipeline – you can adjust resources or replica counts via config and redeploy easily.
Step 10: Scalability and Maintainability Considerations
Finally, design the system to be scalable under load and maintainable over the long term:
Microservice Scalability: Each microservice can be scaled out horizontally. In Azure, this might mean increasing the number of pods in AKS for a busy service or scaling up the App Service plan. Because services are stateless (they don’t keep session-specific data in memory; all state is in SQL or messages), any instance can handle any request. Azure Service Bus decouples producers and consumers, acting as a buffer during peak loads​.
For example, if ticket sales spike, a large number of events queue up; you can increase Settlement service instances to catch up on processing. This elasticity is fundamental to the design.
Performance Optimizations: Use caching where appropriate. E.g., Ticketing service might cache event info or pricing in memory to reduce DB hits. Azure Cache for Redis could be introduced as a quick lookup store if needed (Aspire’s ServiceDefaults even showed an example of adding a Redis cache​).
Just ensure caching doesn’t stale critical data or violate consistency too much.
Monitoring & Logging: Incorporate an Application Performance Management (APM) tool. Azure Application Insights is a good choice – instrument the services to send logs, exceptions, and telemetry. It can correlate requests through the services using distributed tracing (e.g., via the correlation ID we set in Service Bus messages, you can propagate trace IDs). This makes it easier to pinpoint issues in production. Set up dashboards/alerts for error rates, queue lengths (Service Bus has metrics you can alert on if messages back up), and DB performance.
Maintainability - Code: Continue following Clean Architecture patterns. This makes the system easier to extend. For example, if a new requirement comes to support a new payment provider, you can add a new implementation of IBankGateway and perhaps a new microservice if it’s large. If a new domain like "Promotions" or "Loyalty" is needed, you can create a new microservice without affecting existing ones. Each service’s internal code is organized and testable, so refactoring is safer.
Maintainability - DevOps: Use feature flags or configuration for behavior changes. For instance, if integrating a new financial platform, you can toggle it via config. CI/CD pipelines should be maintained and improved as needed – e.g., adding automated security scans (for container vulnerabilities or code analysis).
Security Hardening: Ensure principle of least privilege everywhere. In Azure:
The Service Bus SAS keys should be rotated periodically or use Azure AD roles for access. Each microservice could use a managed identity to access Service Bus (Azure AD integration for Service Bus).
SQL Databases: use separate credentials per service DB, with limited rights (or use managed identity for Azure SQL as well).
Azure AD app registrations: don't use wildcard reply URLs, treat secrets carefully (the Blazor server app secret if any should be in Key Vault or not used if possible).
Enable Azure AD Conditional Access or MFA for user login if appropriate.
Testing Strategies: Apart from unit tests, create integration tests for the whole workflow. For example, a test that simulates a ticket purchase end-to-end: call Ticketing API, then verify that eventually Settlement DB has a record and its status is completed. This can be done in a staging environment with test data. Use the Azure Service Bus in a separate namespace for test or use the dev environment for that.
Documentation & Knowledge: Document the architecture (like this guide!). Also document how to run the system (developers should know to use .NET Aspire for local run), how to deploy (though CI/CD automates it), and how to troubleshoot (e.g., instructions for checking logs or restarting a service).
Clean Architecture Best Practices: Continue enforcing boundaries. For instance, avoid letting the Infrastructure “leak” into Application. If new devs join, have code reviews to ensure they don't, say, call EF Core directly from a Controller bypassing Application layer. Following these rules will keep the codebase healthy and adaptable. Clean Architecture is about making it hard to do the wrong thing by design​, so stick to that structure.
Evolve with Business Needs: The modular design means you can swap components if needed. If in future a different messaging system is required, only the messaging implementation in Infrastructure changes, not the business logic. If a different UI technology is needed, it can be replaced since the backend is fully API-driven. This future-proofs the system.
In conclusion, this ticketing settlement system is scalable, secure, and maintainable. We used a step-by-step approach to outline the architecture (Clean Architecture within each microservice, and microservices connected via an event-driven model​),
the implementation details (folder structures, key code elements), and the infrastructure setup on Azure. By following this guide, a development team can implement the system in an organized manner, and operators can deploy and run it with confidence in Azure's cloud environment.