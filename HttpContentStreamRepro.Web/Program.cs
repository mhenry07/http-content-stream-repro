using HttpContentStreamRepro.Console;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
var app = builder.Build();

app.MapDefaultEndpoints();

app.MapGet("values.csv", async context =>
{
    var options = WriteOptions.Web(app.Logger);
    var stopping = app.Lifetime.ApplicationStopping;
    context.Response.ContentType = "text/csv;charset=utf-8";
    await context.Response.StartAsync(stopping);

    await using var stream = context.Response.Body;
    await Line.WriteLinesAsync(stream, options, stopping);

    await stream.FlushAsync(stopping);
    await context.Response.CompleteAsync();
});

app.Run();
