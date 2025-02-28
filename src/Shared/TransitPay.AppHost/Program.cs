var builder = DistributedApplication.CreateBuilder(args);

// Azure Service Bus
//var serviceBus = builder.AddAzureServiceBus("servicebusnamespace0");

// Ticket Microservice
var ticketMicroservice = builder
    .AddProject<Projects.TicketMicroservice_Api>("ticket-microservice-api")
    //.WithReference(serviceBus)
    .WithHttpEndpoint(name: "ticket-microservice-api-http", port: 5001);

// Transaction Microservice
var transactionMicroservice = builder
    .AddProject<Projects.TransactionMicroservice_Api>("transaction-microservice-api")
    //.WithReference(serviceBus)
    .WithHttpEndpoint(name: "transaction-microservice-api-http", port: 5002);

// Settlement Microservice
var settlementMicroservice = builder
    .AddProject<Projects.SettlementMicroservice_Api>("settlement-microservice-api");

// TransitPay Microservice
var transitPayMicroservice = builder
    .AddProject<Projects.TransitPay_ApiService>("transitpay-microservice-api");

// TransitPay Web
var transitPayWeb = builder
    .AddProject<Projects.TransitPay_Web>("transitpay-web")
    .WithExternalHttpEndpoints()
    .WithReference(transitPayMicroservice)
        .WaitFor(transitPayMicroservice);

builder.AddProject<Projects.IdentityMicroservice_Api>("identitymicroservice-api");

builder.AddProject<Projects.IdentityMicroservice_Api>("identitymicroservice-api");

builder
    .Build()
        .Run();
