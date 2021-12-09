using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using LiteDB;
using UrlShortener.Services;

var shortenService = new ShortenerService();
var host = Host.CreateDefaultBuilder(args)
    .ConfigureWebHostDefaults(builder =>
    {
        builder.ConfigureServices(services =>
        {
            services.AddRouting();
            services.AddSingleton<ILiteDatabase, LiteDatabase>(_ => new LiteDatabase("short-links.db"));
        })
        .Configure(app =>
        {
            app.UseRouting();
            app.UseEndpoints((endpoints) =>
            {
                endpoints.MapGet("/", (ctx) =>
                {
                    return ctx.Response.SendFileAsync("index.html");
                });

                endpoints.MapPost("/shorten", shortenService.HandleShortenUrl);
                endpoints.MapFallback(shortenService.HandleRedirect);
            });
        });
    })
    .Build();

await host.RunAsync();