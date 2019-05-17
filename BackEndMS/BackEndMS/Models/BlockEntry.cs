namespace BackEndMS.Models
{
    using BackEndMS.Helpers;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Windows.Data.Json;

    /// <summary>
    /// Block Query
    /// </summary>
    public class BlockEntry
    {
        public BlockEntry() { }
        public string Id { get; set; }
        /// <summary>
        /// Request ID
        /// </summary>
        public string RequestId { get; set; }

        /// <summary>
        /// 关键词
        /// </summary>
        public string Keywords { get; set; }

        /// <summary>
        /// 处理方式
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public Strategy Strategy { get; set; }

        /// <summary>
        /// 匹配方式
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public Match Match { get; set; }

        /// <summary>
        ///状态
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public Level Level { get; set; }

        /// <summary>
        /// 一级分类
        /// </summary>
        public string Class1 { get; set; }

        /// <summary>
        /// 二级分类
        /// </summary>
        public string Class2 { get; set; }

        /// <summary>
        /// 三级分类
        /// </summary>
        public string Class3 { get; set; }

        /// <summary>
        /// 时效
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public ValidDuration ValidDuration { get; set; }

        /// <summary>
        /// 操作人
        /// </summary>
        public string Operator { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public Status Enable { get; set; }
        
        /// <summary>
        /// 更新时间
        /// </summary>
        public long ModifyDate { get; set; } = LinqHelper.GetUnixTimeStamp(DateTime.Now);
        /// <summary>
        /// 有效时间
        /// </summary>
        public long ExpireDate
        {
            get
            {
                if (ValidDuration == ValidDuration.永久)
                    return 0;
                else
                {
                    var date = LinqHelper.GetDateTime(ModifyDate).AddMonths((int)ValidDuration);
                    return LinqHelper.GetUnixTimeStamp(date);
                }
            }
        }
        public bool selected { get; set; } = false;
    }

    public class BlockEntryCreateModel
    {
       public  BlockEntryCreateModel() {
            BlockEntry = new BlockEntry();
            KeywordsCategory = new List<BlockQueryCategory>();
        }
        public BlockEntryCreateModel(BlockEntry block)
        {
            BlockEntry = new BlockEntry();
            LinqHelper.CopyPropertiesTo(block, BlockEntry);
            KeywordsCategory = new List<BlockQueryCategory>();
        }
        public BlockEntry BlockEntry { get; set; }

        public List<BlockQueryCategory> KeywordsCategory
        {
            get;set;
        }
        public static async Task<List<BlockQueryCategory>> InitCategory()
        {
            string jsonResult = await LinqHelper.GetAllQueryData("BlockQueryCategory");
            var result = JsonConvert.DeserializeObject<JsonResultTemplate<BlockQueryCategory>>(jsonResult);
            if (result.ResultCode == (int)ResultCodeType.操作成功)
            {
                return result.Data.ToList();
            }
            else
                return null;
        }
        /// <summary>
        /// 处理方式列表
        /// </summary>
        public List<Strategy> StrategyList
        {
            get
            {
                var temp = Enum.GetValues(typeof(Strategy)).Cast<Strategy>().ToList();
                return temp;
            }
        }
        /// <summary>
        /// 程度列表
        /// </summary>
        public List<Level> LevelList
        {
            get
            {
                return Enum.GetValues(typeof(Level)).Cast<Level>().ToList();
            }
        }
        /// <summary>
        /// 匹配方式列表
        /// </summary>
        public List<Match> MatchList
        {
            get
            {
                return Enum.GetValues(typeof(Match)).Cast<Match>().ToList();
            }
        }
        /// <summary>
        /// 状态列表
        /// </summary>
        public List<Status> EnableList
        {
            get
            {
                return Enum.GetValues(typeof(Status)).Cast<Status>().ToList();
            }
        }

        /// <summary>
        /// 有效时间列表
        /// </summary>
        public List<ValidDuration> ValidDurationList
        {
            get
            {
                return Enum.GetValues(typeof(ValidDuration)).Cast<ValidDuration>().ToList();
            }
        }
    }

    /// <summary>
    /// 匹配方式
    /// </summary>
    public enum Match
    {
        精确匹配 = 1,
        包含匹配 = 2,
        模糊匹配 = 3
    }

    /// <summary>
    /// 处理方式
    /// </summary>
    public enum Strategy
    {
        例外名单 = 1,
        页面过滤 = 2,
        无操作 = 3
    }

    /// <summary>
    /// 时效 1个月、3个月、永久
    /// </summary>
    public enum ValidDuration
    {
        一个月 = 1,
        三个月 = 2,
        永久 = 0
    }
    public enum Level
    {
        一级 = 1,
        二级 = 2,
        三级 = 3
    }
    public enum Status
    {
        启用 = 1,
        停用 = 0,

    }
    public enum Enable
    {
        有效 = 1,
        无效 = 0,

    }
}
