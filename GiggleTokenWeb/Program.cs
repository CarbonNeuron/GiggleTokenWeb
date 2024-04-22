using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using System.Text.Json.Serialization;
using GiggleTokenWeb;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

var app = builder.Build();

var tokensApi = app.MapGroup("/tokens");
tokensApi.MapGet("/{installationID:guid?}", async (HttpContext http, Guid? installationID, int length = 156) =>
{
    installationID ??= Guid.NewGuid();
    
    var tokenLength = TokenLength.Length156;
    if (length == 112)
    {
        tokenLength = TokenLength.Length112;
    }
    var token = GiggleTokenGenerator.Create(installationID.Value, tokenLength);
    
    
    var acceptHeader = http.Request.Headers["Accept"].ToString();

    // Default to plain text if no specific header is found or it contains 'text/plain'
    bool acceptJson = acceptHeader.Contains("application/json");

    if (acceptJson)
    {

        // Serializing list to JSON and returning it
        http.Response.ContentType = "application/json";
        await http.Response.WriteAsJsonAsync(token, typeof(string), AppJsonSerializerContext.Default);
    }
    else
    {
        // Returning tokens as plain text
        http.Response.ContentType = "text/plain";
        await http.Response.WriteAsync(token);
    }
});
tokensApi.MapGet("/{num:int:min(0):max(1000000)}", async (
    HttpContext http,
    int num,
    int length = 156
    ) =>
{
    var tokenLength = TokenLength.Length156;
    if (length == 112)
    {
        tokenLength = TokenLength.Length112;
    }
    var tokens = new ConcurrentBag<string>();
    Parallel.For(0, num, i =>
    {
        var guid = Guid.NewGuid();
        var token = GiggleTokenGenerator.Create(guid, tokenLength);
        tokens.Add(token);
    });
    
    var acceptHeader = http.Request.Headers["Accept"].ToString();

    // Default to plain text if no specific header is found or it contains 'text/plain'
    bool acceptJson = acceptHeader.Contains("application/json");

    if (acceptJson)
    {

        // Serializing list to JSON and returning it
        http.Response.ContentType = "application/json";
        await http.Response.WriteAsJsonAsync(tokens.ToArray(), typeof(string[]), AppJsonSerializerContext.Default);
    }
    else
    {
        // Returning tokens as plain text
        http.Response.ContentType = "text/plain";
        foreach (var token in tokens)
        {
            await http.Response.WriteAsync(token+"\n");
        }
        
    }
});

app.Run();



[JsonSerializable(typeof(string[]))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{
}