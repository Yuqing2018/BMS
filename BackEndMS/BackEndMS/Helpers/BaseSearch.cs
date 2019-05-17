using Newtonsoft.Json;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BackEndMS.Helpers
{
    public class BaseSearch : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        private void NotifyPropertyChange([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private int _totalCount;
        private int _pageIndex;
        private int _pageSize;
        /// <summary>
        /// 记录总数
        /// </summary>
        //[JsonProperty(PropertyName = "TotalSize")]
        public int TotalCount {
            get
            {
                return _totalCount;
            }
            set
            {
                if(PropertyChanged!=null && value != _totalCount)
                {
                    _totalCount = value;
                    NotifyPropertyChange("TotalCount");
                    NotifyPropertyChange("PageCount");
                    NotifyPropertyChange("isFirst");
                    NotifyPropertyChange("isLast");
                }
            }
        }
        /// <summary>
        /// 当前页数
        /// </summary>
        public  int PageIndex {
            get { return _pageIndex; }
            set {
                if (PropertyChanged != null && value != _pageIndex)
                {
                    _pageIndex = value;
                    NotifyPropertyChange("PageIndex");
                    NotifyPropertyChange("isFirst");
                    NotifyPropertyChange("isLast");
                }
            } }
        /// <summary>
        /// 每页记录数
        /// </summary>
        public int PageSize {
            get { return _pageSize; }
            set
            {
                if (PropertyChanged != null && value != _pageSize)
                {
                    _pageSize = value;
                    NotifyPropertyChange("PageSize");
                    NotifyPropertyChange("PageCount");
                    NotifyPropertyChange("isFirst");
                    NotifyPropertyChange("isLast");
                }
            }
        }

        public int PageCount
        {
            get { return (PageSize!=0 && TotalCount !=0)?((TotalCount % PageSize)==0?(TotalCount / PageSize): (TotalCount / PageSize) + 1) :0; }
        }
        public bool isFirst
        {
            get { return PageIndex <= 0; }
        }
        public bool isLast
        {
            get { return PageIndex >= PageCount-1; }
        }
        /// <summary>
        /// 导出类型
        /// </summary>
        public ExportType? ExportType { get; set; }
    }

    public enum ExportType
    {
        CurrentPage = 1,
        AllPages = 2,
    }
}
