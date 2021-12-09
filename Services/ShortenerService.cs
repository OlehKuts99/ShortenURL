using LiteDB;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using UrlShortener.Helpers;

namespace UrlShortener.Services
{
    public class ShortenerService
    {
        private readonly ShortLinkHelper shortLinkHelper;

        public ShortenerService()
        {
            shortLinkHelper = new ShortLinkHelper();
        }

        public Task HandleShortenUrl(HttpContext context)
        {
            if (!context.Request.HasFormContentType || !context.Request.Form.ContainsKey("url"))
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                return context.Response.WriteAsync("Cannot process request");
            }

            context.Request.Form.TryGetValue("url", out var formData);
            var requestedUrl = formData.ToString();

            if (!Uri.TryCreate(requestedUrl, UriKind.Absolute, out Uri result))
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                return context.Response.WriteAsync("Could not understand URL.");
            }

            var url = result.ToString();
            var liteDB = context.RequestServices.GetService<ILiteDatabase>();
            var links = liteDB.GetCollection<ShortLink>(BsonAutoId.Int32);

            var entry = new ShortLink
            {
                Url = url
            };

            links.Insert(entry);

            var urlChunk = shortLinkHelper.GetUrlChunk(entry.Id);
            var responseUri = $"{context.Request.Scheme}://{context.Request.Host}/{urlChunk}";
            context.Response.Redirect($"/#{responseUri}");

            return Task.CompletedTask;
        }

        public Task HandleRedirect(HttpContext context)
        {
            var database = context.RequestServices.GetService<ILiteDatabase>();
            var collection = database.GetCollection<ShortLink>();
            var path = context.Request.Path.ToUriComponent().Trim('/');
            var id = shortLinkHelper.GetId(path);
            var entry = collection.Find(p => p.Id == id).FirstOrDefault();

            if (entry != null)
            {
                context.Response.Redirect(entry.Url);
            }
            else
            {
                context.Response.Redirect("/");
            }

            return Task.CompletedTask;
        }
    }
}
