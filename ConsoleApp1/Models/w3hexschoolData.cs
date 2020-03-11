using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp1.Models
{
    class w3hexschoolData
    {
        public int keyID { get; set; }
        public string name { get; set; }

        public string blogUrl { get; set; }

        public string updateTime { get; set; }

        public List<page> blogList { get;set;}
    }    
}
