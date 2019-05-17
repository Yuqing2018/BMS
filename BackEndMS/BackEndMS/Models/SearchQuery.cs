using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackEndMS.Models
{
    public class SearchQuery
    {
        [JsonProperty(PropertyName = "Keywords")]
        public string Keywords { get; set; }
        [JsonProperty(PropertyName = "operator")]
        public string Operator { get; set; }
        [JsonProperty(PropertyName = "modifyDate")]
        public long ModifyDate { get; set; }
    }
}
