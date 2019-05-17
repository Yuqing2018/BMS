using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackEndMS.Models
{
    public class JsonSearchResultTemplate<T>
    {
        public string ResultCode { get; set; }
        public DataTemplate<T> Data { get; set; }
        public string Message { get; set; }
    }
    public class DataTemplate<T>
    {
        public int TotalSize { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public IEnumerable<T> Data { get; set; }
    }

    public class JsonResultTemplate<T>
    {
        public string ResultCode { get; set; }
        public IEnumerable<T> Data { get; set; }
        public string Message { get; set; }
    }
}
