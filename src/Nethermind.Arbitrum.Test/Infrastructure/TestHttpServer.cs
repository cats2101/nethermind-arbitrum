// SPDX-License-Identifier: BUSL-1.1
// SPDX-FileCopyrightText: https://github.com/NethermindEth/nethermind-arbitrum/blob/main/LICENSE.md

using System.Net;
using System.Net.Sockets;

namespace Nethermind.Arbitrum.Test.Infrastructure;

public class TestHttpServer : IDisposable
{
    private readonly HttpListener _listener;

    private TestHttpServer(HttpListener listener, string uri)
    {
        _listener = listener;
        Uri = uri;
    }

    public string Uri { get; }

    public static TestHttpServer Start()
    {
        HttpListener listener = new();
        string uri = GetLocalhostUri();
        listener.Prefixes.Add(uri);
        listener.Start();

        return new TestHttpServer(listener, uri);
    }

    public async Task Handle(Func<string, byte[]> handle, string contentType = "application/json")
    {
        HttpListenerContext ctx = await _listener.GetContextAsync();
        using StreamReader reader = new(ctx.Request.InputStream);
        string body = await reader.ReadToEndAsync();

        byte[] response = handle(body);

        ctx.Response.ContentType = contentType;
        ctx.Response.ContentLength64 = response.Length;
        await ctx.Response.OutputStream.WriteAsync(response);
        ctx.Response.Close();
    }

    private static string GetLocalhostUri()
    {
        using TcpListener tcp = new(IPAddress.Loopback, 0);
        tcp.Start();
        int port = ((IPEndPoint)tcp.LocalEndpoint).Port;
        tcp.Stop();
        return $"http://localhost:{port}/";
    }

    public void Dispose()
    {
        ((IDisposable)_listener).Dispose();
    }
}
