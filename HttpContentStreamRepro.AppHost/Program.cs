var builder = DistributedApplication.CreateBuilder(args);

var web = builder.AddProject<Projects.HttpContentStreamRepro_Web>("web");

builder.AddProject<Projects.HttpContentStreamRepro_Console>("console")
    .WithReference(web)
    .WaitFor(web);

builder.Build().Run();
