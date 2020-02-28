using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp1.Models
{
    class w3hexschoolDataWithMeta
    {
        public string name { get; set; }

        public string blogUrl { get; set; }

        public string updateTime { get; set; }

        public IEnumerable<Meta> blogList { get; set; }
    }
}
