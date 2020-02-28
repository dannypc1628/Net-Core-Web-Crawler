using ConsoleApp1.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Target
    {
        HttpClient HttpClient { get; }
        IHttpClientFactory httpClientFactory { get; }
        public Target(IHttpClientFactory _httpClientFactory)
        {            
            HttpClient = _httpClientFactory.CreateClient();
            httpClientFactory = _httpClientFactory;
        }

        public async Task JsonDecode(string dataString)
        {
            var list = JsonSerializer.Deserialize<List<w3hexschoolData>>(dataString);
            var newList = new List<w3hexschoolDataWithMeta>();
           
            foreach (var item in list)
            {                
                Console.WriteLine(item.name);
                var newW3hexSchoolData = new w3hexschoolDataWithMeta();
                var newBlogList = new List<Meta>();
                if (item.blogList != null)
                {
                    foreach (var page in item.blogList)
                    {
                        Crawler crawler = new Crawler(httpClientFactory);

                        Console.Write("    ");
                        Console.WriteLine(page.title + "    " + page.url);
                        Console.WriteLine();
                        
                        Meta meta = await crawler.Get(page.url);
                        meta.url = page.url;
                        if (true == string.IsNullOrEmpty(meta.title))
                        {
                            meta.title = page.title;
                        }
                        newBlogList.Add(meta);
                    }
                }
                newW3hexSchoolData.blogList = newBlogList;
                newW3hexSchoolData.name = item.name;
                newW3hexSchoolData.updateTime = item.updateTime;
                newW3hexSchoolData.blogUrl = item.blogUrl;
                newList.Add(newW3hexSchoolData);
            }
            SaveToJson(newList);
        }
        private void SaveToJson<T>(IEnumerable<T> data)
        {
            Console.WriteLine("進行SaveToJson");
            var json = JsonSerializer.Serialize<IEnumerable<T>>(data);

            string docPath =
         Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            // Write the string array to a new file named "WriteLines.txt".
            using (StreamWriter outputFile = new StreamWriter(Path.Combine(docPath, "WriteLines.txt")))
            {
                outputFile.WriteLine(json);
            }
            Console.WriteLine("儲存完畢");
        }

        public async Task<string> Get(string url)
        {
            var responseMessage = await HttpClient.GetAsync(url);
            if (true == responseMessage.IsSuccessStatusCode)
            {
                var dataResult = await responseMessage.Content.ReadAsStringAsync();
                //var list = JsonSerializer.Deserialize<List<object>>(dataResult);
                                
                return dataResult;
            }
            else{
                if (responseMessage.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    Console.WriteLine("404 NotFound");
                    return "";
                }
                else
                {
                    Console.WriteLine(responseMessage.StatusCode);
                    return "";
                }
            }
        }

        
    }
}
