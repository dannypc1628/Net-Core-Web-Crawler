using AngleSharp;
using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Crawler
    {        
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
        
        public async Task Get(string url)
        {
            var config = Configuration.Default;
            //var context = BrowsingContext.New(config);
            var context = BrowsingContext.New(config.WithDefaultLoader());//要發送請求用這個

            //var document = await context.OpenAsync(req => req.Content("<h1>Some example source</h1><p>This is a paragraph element"));
            Console.WriteLine($"連線至{url}");
            var document = await context.OpenAsync(url);
            //Console.WriteLine(document.ToHtml());
            var head = document.QuerySelector("head");
            var metas = head.QuerySelectorAll("*[property='og:image']");
            //Console.WriteLine(head.ToHtml());
                      

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
            //Console.WriteLine(document.DocumentElement.OuterHtml);  

            Console.WriteLine($"url {data.url}");
            Console.WriteLine($"title {data.title}");
            Console.WriteLine($"site_name {data.site_name}");
            Console.WriteLine($"image {data.image}");
            Console.WriteLine($"description {data.description}");
            Console.WriteLine($"updated_time {data.updated_time}");
            Console.WriteLine($"published_time {data.published_time}");
            
        }
    }
}
