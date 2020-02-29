using ConsoleApp1.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        public async Task<Meta> CrawlingPageAsync(string url, string title)
        {
            Crawler crawler = new Crawler(httpClientFactory);

            Meta meta = await crawler.Get(url);
            meta.url = url;
            if (true == string.IsNullOrEmpty(meta.title))
            {
                meta.title = title;
            }
            return meta;
        }
        public async Task<w3hexschoolDataWithMeta> CrawlingMainAsync(w3hexschoolData data)
        {
            var _w3hexschoolDataWithMeta = new w3hexschoolDataWithMeta();
            
            Meta[] metas = new Meta[] { };
            if (data.blogList != null)
            {
                Task<Meta>[] allTasks = new Task<Meta>[data.blogList.Count];
                int i = 0;
                foreach (var page in data.blogList)
                {
                    allTasks[i] = CrawlingPageAsync(page.url, page.title);
                    i++;
                }
                try
                {
                    metas = await Task.WhenAll(allTasks);
                    
                    //Task.WaitAll(allTasks);
                }
                catch (Exception exc)
                {
                    Console.WriteLine($"CrawlingPageAsync捕捉到例外異常的物件型別為 : {exc.GetType().Name}");
                    // 當所有等候工作都執行結束後，可以檢查是否有執行失敗的工作
                    foreach (Task faulted in allTasks.Where(t => t.IsFaulted))
                    {
                        Console.WriteLine(faulted.Exception.InnerException.Message);
                    }
                }
            }
            _w3hexschoolDataWithMeta.blogList = metas;
            _w3hexschoolDataWithMeta.name = data.name;
            _w3hexschoolDataWithMeta.updateTime = data.updateTime;
            _w3hexschoolDataWithMeta.blogUrl = data.blogUrl;

            return _w3hexschoolDataWithMeta;
        }

        public async Task JsonDecode(string dataString)
        {
            var list = JsonSerializer.Deserialize<List<w3hexschoolData>>(dataString);
            

            Task<w3hexschoolDataWithMeta>[] allTasks = new Task<w3hexschoolDataWithMeta>[list.Count];
            var i = 0;
            w3hexschoolDataWithMeta[] result = new w3hexschoolDataWithMeta[0];
            foreach (var item in list)
            {
                //Console.WriteLine(item.name);
                allTasks[i] = CrawlingMainAsync(item);                
                i++;
            }
            try
            {
                result = await Task.WhenAll(allTasks);               
            }
            catch (Exception exc)
            {
                Console.WriteLine($"CrawlingMainAsync捕捉到例外異常的物件型別為 : {exc.GetType().Name}");
                // 當所有等候工作都執行結束後，可以檢查是否有執行失敗的工作
                foreach (Task faulted in allTasks.Where(t => t.IsFaulted))
                {
                    Console.WriteLine(faulted.Exception.InnerException.Message);
                }
            }

            SaveToJson(result);
        }
        private void SaveToJson<T>(IEnumerable<T> data)
        {
            Console.WriteLine("進行SaveToJson");
            var json = JsonSerializer.Serialize<IEnumerable<T>>(data);

            string docPath = Directory.GetCurrentDirectory();

            File.WriteAllText(Path.Combine(docPath, "data.json"), json);

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
            else
            {
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
