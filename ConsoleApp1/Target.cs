using ConsoleApp1.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Target
    {
        HttpClient httpClient { get; }
        IHttpClientFactory httpClientFactory { get; }
        public Target(IHttpClientFactory _httpClientFactory)
        {
            httpClient = _httpClientFactory.CreateClient();
            httpClientFactory = _httpClientFactory;
        }
        public async Task<Meta> CrawlingPageAsync(string url, string title , Crawler crawler)
        {
            
            Meta meta = new Meta();
            try
            {                
                meta = await crawler.Get(url);                
            }
            catch(Exception ex)
            {
                Console.WriteLine($"crawler捕捉到例外異常的物件型別為 : {ex.GetType().Name}");
                Console.WriteLine("crawler捕捉到例外異常 is: " +
                    ex.GetType().ToString() +
                    " –Message: " + ex.Message +
                    " -- Inner Message: " +
                    ex.InnerException.Message 
                    );
            }

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
            
            List<Meta> metas = new List<Meta>();
            if (data.blogList != null)
            {
                Crawler crawler = new Crawler(httpClientFactory);
                
                foreach (var page in data.blogList)
                {
                    metas.Add ( await CrawlingPageAsync(page.url, page.title, crawler));                    
                }                
            }
            _w3hexschoolDataWithMeta.blogList = metas;
            _w3hexschoolDataWithMeta.keyID = data.keyID;
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
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                docPath = docPath + "\\Output"; 
            }
            else
            {
                docPath += "/Output";
            }

            File.WriteAllText(Path.Combine(docPath, "data.json"), json);

            Console.WriteLine("儲存完畢");
        }

        public async Task<string> Get(string url)
        {
            var retry = new RetryWithExponentialBackoff();
            var dataResult = "";
            try
            {
                await retry.RunAsync(async () =>
                {
                    var responseMessage = await httpClient.GetAsync(url);
                    Console.WriteLine(responseMessage.StatusCode);
                    if (true == responseMessage.IsSuccessStatusCode)
                    {
                        dataResult = await responseMessage.Content.ReadAsStringAsync();
                    }
                    else
                    {
                        dataResult = responseMessage.StatusCode.ToString();
                    }
                });
            }
            catch (Exception ex)
            {
                dataResult = $"{ex.Message} + {ex.InnerException.Message}";
                Console.WriteLine(dataResult);
            }
            return dataResult;
        }


    }
}
