using AngleSharp;
using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;
using ConsoleApp1.Models;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleApp1
{
    class Crawler
    {
        readonly HttpClient httpClient;
        
        public Crawler(IHttpClientFactory _httpClientFactory)
        {
            httpClient = _httpClientFactory.CreateClient();
        }
        private string SelectScript(string propertieyName)
        {
            string start = "*[property = '";
            string og = "og:";
            string article = "article:";
            string end = "']";

            var s = "";
            if (propertieyName == "published_time")
            {
                s = $"{start}{article}{propertieyName}{end} ";
            }
            else
            {
                s = $"{start}{og}{propertieyName}{end} ";
            }

            return s;
        }
        
        public async Task<Meta> Get(string url)
        {
            var retry = new RetryWithExponentialBackoff();
            HttpResponseMessage responseMessage ;
            var responseResult = "";
            Meta meta = new Meta();

            try {
                await retry.RunAsync(async () =>
                {
                    responseMessage = await httpClient.GetAsync(url);
                    if (responseMessage.IsSuccessStatusCode)
                    {
                        responseResult =  responseMessage.Content.ReadAsStringAsync().Result;                        
                    }
                    else
                    {
                        if (responseMessage.StatusCode != System.Net.HttpStatusCode.OK)
                        {
                            Console.WriteLine(responseMessage.StatusCode);
                        }
                    }
                    meta.http_status_code = (int)responseMessage.StatusCode;
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message+ ex.InnerException.Message);
                meta.http_status_code = 500;
            }
            
            
            var config = Configuration.Default;
           
            var context = BrowsingContext.New(config);
                        
            Console.WriteLine($"準備連線至{url}");
            var document = await context.OpenAsync(res=>res.Content(responseResult));
            //Console.WriteLine(document.ToHtml()); //顯示抓取document資料 
            //Console.WriteLine(document.DocumentElement.OuterHtml);//顯示抓取document資料 

            var head = document.QuerySelector("head");
            //Console.WriteLine(head.ToHtml());  //顯示抓取head資料                    

            
            var type = meta.GetType();
            var properties = type.GetProperties();

            foreach (var property in properties)
            {
                var note = head.QuerySelector(SelectScript(property.Name));
                //Console.WriteLine(property.Name);
                if (note != null)
                {
                   // Console.Write($"     ");
                   // Console.WriteLine(note.GetAttribute("content"));
                    property.SetValue(meta,note.GetAttribute("content"));
                }
                else
                {
                   // Console.WriteLine("找不到");
                }
            }

            Console.WriteLine("顯示Meta物件的資料");
            Console.WriteLine($"url {meta.url}");
            Console.WriteLine($"title {meta.title}");
            Console.WriteLine($"site_name {meta.site_name}");
            Console.WriteLine($"image {meta.image}");
            Console.WriteLine($"description {meta.description}");
            Console.WriteLine($"updated_time {meta.updated_time}");
            Console.WriteLine($"published_time {meta.published_time}");

            return meta;
        }
    }
}
