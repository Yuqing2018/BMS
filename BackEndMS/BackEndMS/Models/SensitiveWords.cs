using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BackEndMS.Models
{
    public class AssociativeWord : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        private SensitiveStatus _Status;
        private int _cycleNum = 1;
        public string Text { get; set; }
        public long LastDatetime { get; set; }
        //[JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public SensitiveStatus Status
        {
            get { return _Status; }
            set
            {
                if (PropertyChanged != null && _Status != value)
                {
                    _Status = value;
                    OnPropertyChanged("Status");
                }
                
            }
        }
        public string ResultData { get; set; }
        public int cycleNum
        {
            get { return _cycleNum; }
            set
            {
                if (PropertyChanged != null && _cycleNum != value)
                {
                    _cycleNum = value;
                    OnPropertyChanged("cycleNum");
                }
                    
               
            }
        }

        private void OnPropertyChanged([CallerMemberName]string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        }
    }
    public enum SensitiveStatus
    {
        未处理 = 1,
        获取AS树 = 2,
        已获取 = 3,
        处理中 = 4,
        已处理 = 5,
        完结 = 6,
    }

    public class SensitiveWords
    {
        public SensitiveWords()
        {
            Children = new List<SensitiveWords>();
        }
        public string Text { get; set; }
        public bool isSelected { get; set; } = false;
        public List<SensitiveWords> Children { get; set; }
    }

    public class BingWSItem
    {
        public string Content { get; set; }
        public QueryEntryFlag Flag { get; set; }
        public bool IsBlock { get; set; }
        public long BlockDate { get; set; }
        public string Operator { get; set; }

    }
}
