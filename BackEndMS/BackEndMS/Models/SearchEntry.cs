using BackEndMS.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BackEndMS.Models
{
    public class SearchEntry: BaseSearch
    {
        public SearchEntry()
        {
            SearchEntity = new BlockEntry();
        }
        public BlockEntry SearchEntity { get; set; }

        public QueryEntryFlag Flag { get; set; }
        public SubmitStatus Status { get; set; } = SubmitStatus.全部;
        /// <summary>
        /// 查询起始时间
        /// </summary>
        public DateTime? StartDate { get; set; }
        /// <summary>
        /// 查询结束时间
        /// </summary>
        public DateTime? EndDate { get; set; }
        /// <summary>
        /// 是否过期
        /// </summary>
        public Boolean? IsExpire { get; set; }

        public List<QueryEntryFlag> FlagList
        {
            get
            {
                return Enum.GetValues(typeof(QueryEntryFlag)).Cast<QueryEntryFlag>().ToList();
            }
        }
        public List<SubmitStatus> SubmitStatusList
        {
            get
            {
                return Enum.GetValues(typeof(SubmitStatus)).Cast<SubmitStatus>().ToList();
            }
        }
    }
}
