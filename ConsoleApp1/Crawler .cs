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
            var responseMessage = await httpClient.GetAsync(url);
            var d = "";
            if (responseMessage.IsSuccessStatusCode)
            {
                 d = responseMessage.Content.ReadAsStringAsync().Result;
            }
            var config = Configuration.Default;
           
            var context = BrowsingContext.New(config);
                        
            Console.WriteLine($"準備連線至{url}");
            var document = await context.OpenAsync(res=>res.Content(d));
            //Console.WriteLine(document.ToHtml()); //顯示抓取document資料 
            //Console.WriteLine(document.DocumentElement.OuterHtml);//顯示抓取document資料 

            var head = document.QuerySelector("head");
            //Console.WriteLine(head.ToHtml());  //顯示抓取head資料                    

            var data = new Meta();
            var type = data.GetType();
            var properties = type.GetProperties();

            foreach (var property in properties)
            {
                var note = head.QuerySelector(SelectScript(property.Name));
                Console.WriteLine(property.Name);
                if (note != null)
                {
                    Console.Write($"     ");
                    Console.WriteLine(note.GetAttribute("content"));
                    property.SetValue(data,note.GetAttribute("content"));
                }
                else
                {
                    Console.WriteLine("找不到");
                }
            }

            Console.WriteLine("顯示Meta物件的資料");
            Console.WriteLine($"url {data.url}");
            Console.WriteLine($"title {data.title}");
            Console.WriteLine($"site_name {data.site_name}");
            Console.WriteLine($"image {data.image}");
            Console.WriteLine($"description {data.description}");
            Console.WriteLine($"updated_time {data.updated_time}");
            Console.WriteLine($"published_time {data.published_time}");

            return data;
        }
    }
}
