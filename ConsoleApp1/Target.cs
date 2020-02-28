﻿using ConsoleApp1.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Target
    {
        HttpClient HttpClient { get; }
        public Target()
        {
            var serviceProvider = new ServiceCollection().AddHttpClient().BuildServiceProvider();

            var httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();

            HttpClient = httpClientFactory.CreateClient();
        }

        public void JsonDecode(string dataString)
        {
            var list = JsonSerializer.Deserialize<List<w3hexschoolData>>(dataString);
            foreach (var item in list)
            {
                Console.WriteLine(item.name);

                if (item.blogList != null)
                {
                    foreach (var page in item.blogList)
                    {
                        Console.Write("    ");
                        Console.WriteLine(page.title + "    " + page.url);
                        Console.WriteLine();
                    }
                }
            }
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