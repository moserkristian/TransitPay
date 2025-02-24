var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.TransitPay_ApiService>("apiservice");

builder.AddProject<Projects.TransitPay_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
