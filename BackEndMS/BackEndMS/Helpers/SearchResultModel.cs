using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace BackEndMS.Helpers
{
    #region 初始模拟使用 暂时无用
    public class SearchResultModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        private void NotifyPropertyChange(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        public SearchResultModel()
        {
            rsList = new ObservableCollection<SearchUrlItem>();
        }
        public String RequestID { get; set; }
        public int QueryCount { get; set; }
        public int UrlCount { get; set; }
        public int RsCount { get; set; }
        public ObservableCollection<SearchUrlItem> rsList { get; set; }
    }
    public class SearchUrlItem: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        private void NotifyPropertyChange(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        public int searchNo { get; set; }
        public String query { get; set; }
        public string url { get; set; }
        public string rs { get; set; }
        public String userName { get; set; }
        public String queryTime { get; set; }

    }
    #endregion
    public class BaseSearchModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        private void NotifyPropertyChange(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        private string _RequestID;
        public string RequestID {
            get
            {
                return _RequestID;
            }
            set
            {
                if (PropertyChanged != null && value != _RequestID)
                {
                    _RequestID = value;
                    NotifyPropertyChange("RequestID");
                }
            } }
        public string userName { get; set; }
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
    }
}
