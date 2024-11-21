# http-content-stream-repro

This repo implements a reproduction of an issue where a certain pattern of reading a .NET HttpClient content stream
results in data corruption.

See: [dotnet/aspire#6745](https://github.com/dotnet/aspire/issues/6745)

## Running

To run the application, start the HttpContentStreamRepro.AppHost project and in the Aspire dashboard, open the logs for
the "console" project. On my machine with the default options, this repro tends to result in data corruption within 20
seconds. An exception will be thrown and/or logged when a corrupt line is detected.

Example log when corruption occurs:

```
Reader: Line was corrupted at row 73,036, ~6,416,181 bytes:
Actual:    'abc,73036,def,CCC,ghi,01/01/0001 00:01:13abc,73032,def,YYYYYYYYYYYY,ghi,01/01/0001 00:01:13 +00:00,jkl,01/01/0001 20:17:12 +00:00,mno'
Expected:  'abc,73036,def,CCC,ghi,01/01/0001 00:01:13 +00:00,jkl,01/01/0001 20:17:16 +00:00,mno'
```

In the above case, the actual line is corrupted in that it contains text from two different lines. In some other cases,
sometimes it fails by returning text from the wrong line. This only appears to happen when using the HttpClient but not
when using the local stream (see `StreamSource` under below options).

## Options

- StreamSource: Whether to use the HttpClient or to use a local reference stream (for comparison). Default: Http.
- FillBuffer: Whether to read data into the buffer until it's full before processing lines (true), or to process
  lines after each read call (false). `FillBuffer: true` is comparable to Google's MediaDownloader implementation and
  appears to trigger the issue. Default: true.
- ChunkSize: The size of the buffer to use for reading and processing the stream and processing lines. Large values
  seem more likely to trigger the issue. Default: 4_000_000 (4 MB).
- BatchSize: The number of lines to process before performing some fake "I/O". Default: 100.
- Delay: The length of time to wait to simulate fake "I/O". Default: 15 milliseconds.

The default options trigger the issue when I run the repro on my machine.

### Toggling the Aspire Proxy

In the AppHost Program.cs, there's also an `isProxied` variable that toggles the Aspire proxy. `true` is the default to
use the Aspire proxy, which triggers the issue. Setting it to `false` will disable the proxy for the web resource and
does not trigger the issue.

## The Solution

The repro solution contains an ASP.NET Core web project which streams deterministic lines via an HttpResponse.Body
stream, and a console application which reads and processes the lines via an HttpClient content stream. The main code
to reproduce the issue is in `Reader.cs` within the console project, and options can be set from `Program.cs` within
the console project. Within Reader, `ReadStreamAsync` and `ReadUntilFullAsync` are particularly relevant.

## Background

The issue was originally observed when processing download streams using
[Google Cloud Storage](https://cloud.google.com/dotnet/docs/reference/Google.Cloud.Storage.V1) via an implementation of
[System.IO.Pipelines](https://learn.microsoft.com/en-us/dotnet/standard/io/pipelines) against a local Docker container
running [fake-gcs-server](https://github.com/fsouza/fake-gcs-server).

This repro removes those dependencies, and uses a simplified but comparable implementation. In particular, when the
options `FillBuffer: true` and `StreamSource: StreamSource.Http` are used, the implementation is comparable to Google's
[MediaDownloader](https://github.com/googleapis/google-api-dotnet-client/blob/main/Src/Support/Google.Apis/Download/MediaDownloader.cs)
implementation.
