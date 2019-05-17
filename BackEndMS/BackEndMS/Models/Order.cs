using BackEndMS.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;

namespace BackEndMS.Models
{
    public class Order
    {
        public Order()
        {
            TempLables = new ObservableCollection<TempLable>();
            CelaExplains = new ObservableCollection<CelaExplain>();
            FileContents = new ObservableCollection<UploadImage>();
            FileDetails = new ObservableCollection<UploadImage>();
            BaiduActions = new ObservableCollection<UploadImage>();
        }
        public string Id { get; set; }
        /// <summary>
        /// 指令来源
        /// </summary>
        [DisplayName("指令来源")]
        public string Source { get; set; }
        [DisplayName("指令序号")]
        public string OrderId { get; set; }
        /// <summary>
        /// 指令类别 一级
        /// </summary>
        [DisplayName("类别")]
        public string Class1 { get; set; }
        private List<string> _Labels = new List<string>();
        /// <summary>
        /// 标签 可修改，可添加
        /// </summary>
        [DisplayName("标签")]
        public List<string> Labels
        {
            get
            {
                if(TempLables != null && TempLables.Count >0)
                {
                    return TempLables.Select(x => x.Text).Where(x=>!String.IsNullOrWhiteSpace(x)).Distinct().ToList();
                }
                else
                {
                    return _Labels;
                }
                
            }
            set
            {
                _Labels = value;
            }
        }
        [DisplayName("处理方式")]
        public List<string> Strategy { get; set; }
        [DisplayName("指令内容")]
        public string TextContent { get; set; }
        [DisplayName("指令内容附件")]
        public ObservableCollection<UploadImage> FileContents { get; set; }
        [DisplayName("指令详情")]
        public string TextDetail { get; set; }
        [DisplayName("指令详情附件")]
        public ObservableCollection<UploadImage> FileDetails { get; set; }
        [DisplayName("指令截图附件")]
        public ObservableCollection<UploadImage> BaiduActions { get; set; }
        [DisplayName("Cela建议")]
        public ObservableCollection<CelaExplain> CelaExplains { get; set; }
        [DisplayName("操作人")]
        public string Operator { get; set; }
        [DisplayName("操作时间")]
        public long ModifyDate { get; set; }
        [JsonIgnore]
        public ObservableCollection<TempLable> TempLables {get;set; }

        [JsonIgnore]
        public bool selected { get; set; } = false;
        [JsonIgnore]
        public int CelaCounts { get {
                return CelaExplains.Count();
            } }

    }
    public enum OrderSourceType
    {
        CAC = 1,
        BJIO = 2,
        BJPSB = 3,
        MPS = 4,
        BJAIC = 5,
        地方网信办 = 6,
        用户投诉 = 7,
        文化执法大队 = 8,
    }
    public enum OrderStrategyType
    {
        摘除指定链接 = 1,
        指向白名单 = 2,
        关闭搜索衍生品 = 3,
        样本过滤 = 4,
        摘除图片 = 5,
        禁搜 = 6,
        屏蔽Domain = 7,
        关闭网典 = 8,
        关闭词典 = 9
    }

    public enum OrderCategoryType
    {
        涉政 = 1,
        社会事件 = 2,
        违禁品 = 3,
        色情 = 4,
        广告 = 5,
        通知 = 6,
    }
    public class CelaExplain
    {
        public string Item { get; set; }
        public List<string> Strategy { get; set; }
        [JsonIgnore]
        public List<String> CelaStrategyList
        {
            get
            {
                return Enum.GetNames(typeof(CelaStrategyType)).ToList();
            }
        }
        [JsonIgnore]
        public bool IsEditable { get; set; } = true;
    }
    public enum CelaStrategyType
    {
        不回复 = 0,
        摘除指定链接 = 1,
        URL过滤 = 2,
        移除AS_RS = 3,
        摘除图片 = 4,
        样本过滤 = 5,
        加入例外名单 = 6,
        屏蔽Domain = 7,
        手动输入 = 8,
    }

    public class UploadImage:INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this,new PropertyChangedEventArgs(propertyName));
        }
        private BitmapImage _ImageSource;
        private string _ImageTitle;
        [JsonIgnore]
        public BitmapImage ImageSource
        {
            get
            {
                return _ImageSource;
            }
            set
            {
                _ImageSource = value;
                if (null != PropertyChanged)
                    OnPropertyChanged("ImageSource");
            }
        }
        [JsonIgnore]
        public StorageFile UplodFile { get; set; }
        [JsonIgnore]
        public string ImageTitle
        {
            get
            {
                return _ImageTitle;
            }
            set
            {
                _ImageTitle = value;
                if (null != PropertyChanged)
                    OnPropertyChanged("ImageTitle");
            }
        }
        public string ContentType { get; set; }
        public string Id { get; set; }
        public string FileExtension { get; set; }
        [JsonIgnore]
        public bool IsEdit { get { return !String.IsNullOrEmpty(Id); } }


    }
    public class TempLable
    {
        public string Text { get; set; } = string.Empty;
    }

    public class OrderSearch: BaseSearch
    {
        public string Source { get; set; }
        public string OrderId { get; set; }
        public string Class1 { get; set; }
        public string Strategy { get; set; }
        public string Label { get; set; }

        public string TextContent { get; set; }
        public string TextDetail { get; set; }
        public string Operator { get; set; }
        /// <summary>
        /// 查询起始时间
        /// </summary>
        public DateTime? StartDate { get; set; }
        /// <summary>
        /// 查询结束时间
        /// </summary>
        public DateTime? EndDate { get; set; }
    }
}
