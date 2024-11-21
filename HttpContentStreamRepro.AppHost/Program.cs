var isProxied = true;

var builder = DistributedApplication.CreateBuilder(args);

var web = isProxied
    ? builder.AddProject<Projects.HttpContentStreamRepro_Web>("web")
    : builder.AddProject<Projects.HttpContentStreamRepro_Web>("web", launchProfileName: null)
        .WithHttpEndpoint(port: 5251, isProxied: false);

builder.AddProject<Projects.HttpContentStreamRepro_Console>("console")
    .WithReference(web)
    .WaitFor(web);

builder.Build().Run();
