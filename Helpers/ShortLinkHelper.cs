using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UrlShortener.Helpers
{
    public class ShortLinkHelper
    {
        public string GetUrlChunk(int id)
        {
            return WebEncoders.Base64UrlEncode(BitConverter.GetBytes(id));
        }

        public int GetId(string urlChunk)
        {
            return BitConverter.ToInt32(WebEncoders.Base64UrlDecode(urlChunk));
        }
    }
}
