using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackEndMS.Models
{
    public class BlockQueryCategory
    {
        public BlockQueryCategory()
        {
            Children = new List<BlockQueryCategory>();
        }
        public string Id { get; set; }
        public string Name { get; set; }
        [JsonIgnoreAttribute]
        public int OrderIndex { get; set; }
        public List<BlockQueryCategory> Children { get; set; }
    }
}
