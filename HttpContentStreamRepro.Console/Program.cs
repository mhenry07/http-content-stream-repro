using HttpContentStreamRepro.Console;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);
builder.AddServiceDefaults();

builder.Services
    .Configure<ReaderOptions>(options =>
    {
        options.BatchSize = 100;
        options.ChunkSize = 4_000_000;
        options.Delay = TimeSpan.FromMilliseconds(15);
        options.FillBuffer = true;
        options.StreamSource = StreamSource.Http;
    })
    .AddTransient<Reader>()
    .AddHttpClient<Reader>(client => client.BaseAddress = new("http://web"));

var app = builder.Build();

await using var reader = app.Services.GetRequiredService<Reader>();
await using var stream = await reader.GetStreamAsync();

await reader.ReadStreamAsync(stream);
