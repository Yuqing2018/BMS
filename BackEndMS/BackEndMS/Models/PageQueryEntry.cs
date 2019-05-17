using System;
using System.ComponentModel;

namespace BackEndMS.Models
{
    public class PageQueryEntry
    {
        public string Id { get; set; }
        public PageQueryEntry() { }
        [DisplayName("RequestId")]
        public string RequestId { get; set; }
        [DisplayName("关键词")]
        public string Keywords { get; set; }
        [DisplayName("内容")]
        public string Content { get; set; }
        [DisplayName("内容标识")]
        public QueryEntryFlag Flag { get; set; }
        [DisplayName("操作人")]
        public string Operator { get; set; }
        [DisplayName("操作时间")]
        public long ModifyDate { get; set; }
        public bool isFirst { get; set; }
        [DisplayName("状态")]
        public SubmitStatus Status { get; set; }
        [DisplayName("当前提交")]
        public string[] SubmitTo { get; set; }
        //"Block","UnBlock"
        public String Method { get; set; }
        [DisplayName("有效期限")]
        public int ValidDays { get; set; }
        [DisplayName("是否过期")]
        public Boolean IsExpire { get; set; } = false;

    }

    public enum QueryEntryFlag
    {
        All = 0,//界面中显示使用
        Url = 1,//Url
        QS = 2,//右侧相关词语
        AS = 3,//搜索框联想词
        PQS = 4,//图片搜索相关词
        PIQS = 5,//图片搜索中用户感兴趣的相关词
        Domain = 6,//Url_Domain 类型
    }
    public enum SubmitStatus
    {
        未提交 = 0,
        已提交 = 1,
        已生效 = 2,
        全部 = 3,
    }
    public enum ValidDays
    {
        永久 = 0,
        一个月 = 30,
        三个月 = 90,
    }
}
