using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;

namespace ConsoleApp1
{
    class Program
    {
        static async Task Main(string[] args)
        {            
            string url ;
            url = "https://dannyliu.me/%e5%9c%a8%e7%8f%be%e6%9c%89asp-net-mvc%e5%b0%88%e6%a1%88%e4%b8%ad%e5%8a%a0%e5%85%a5asp-net-identity/";
                       
            Crawler c = new Crawler();
            Meta m = await c.Get(url);

            Console.WriteLine("End~~~");
            Console.Read();
        }        
    }
}
