using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace ConsoleApp1
{
    class Checker
    {
        public static string UrlWithHost(string url, HttpResponseMessage responseMessage)
        {
            Uri requestUri = responseMessage.RequestMessage.RequestUri;
            
            if (requestUri.Scheme != "https")
            {
                return "";
            }
            if(url[0] == '/')
            {               
                return $"{requestUri.Scheme}://{requestUri.Host}{url}";
            }
            else
            {
                return url;
            }
            
        }
    }
}
