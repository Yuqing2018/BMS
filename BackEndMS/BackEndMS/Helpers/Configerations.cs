using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Windows.UI.Xaml.Controls;

namespace BackEndMS.Helpers
{
    public static class ConstStrings
    {
        public const string EmailAddressRegex = @"^[a-z0-9]+([._\\-]*[a-z0-9])*@([a-z0-9]+[-a-z0-9]*[a-z0-9]+.){1,63}[a-z0-9]+$";
    }
    public class Configerations
    {
        private List<Scenario> _Scenarios = new List<Scenario>()
        {
             new Scenario()
            {
                Title ="页面巡检",
                Symbol=Symbol.Scan,
                ClassType=typeof(Query_MainPage),
                Tabs = new List<ModelPivotItem>()
                {
                    new ModelPivotItem(){HeaderTitle="操作页面",CalssType=typeof(QueryContent),isSelected=true},
                    new ModelPivotItem(){HeaderTitle="提交记录",CalssType=typeof(RecordIndex),isSelected=false},
                    new ModelPivotItem(){HeaderTitle="巡检记录",CalssType=typeof(SearchQueryPage),isSelected=false},
                }
            }, new Scenario()
            {
                Title ="词表管理",
                ClassType=typeof(BackManage_Query),
                Symbol=Symbol.Manage,
            }, new Scenario()
            {
                Title ="指令管理",
                ClassType=typeof(OrderStatisticsPage),
                Symbol=Symbol.AllApps,
            }
            , new Scenario()
            {
                Title ="敏感词扩展",
                ClassType = typeof(BingWSPage),
                //ClassType=typeof(SensitiveWordExtractionPage),
                Symbol=Symbol.ReportHacked,
            }, new Scenario()
            {
                Title ="用户管理",
                ClassType=typeof(UserIndex),
                Symbol=Symbol.People,
            }
        };
        public ObservableCollection<Scenario> Scenarios
        {
            get { return _Scenarios.ToObservableCollectionAsync(); }
        }

    }

    public class Scenario
    {
        public string Title { get; set; }
        public Type ClassType { get; set; }
        public Symbol Symbol { get; set; }
        public List<ModelPivotItem> Tabs { get; set; }
    }

    public class ModelPivotItem : INotifyPropertyChanged
    {
        public string HeaderTitle { get; set; }
        public Type CalssType { get; set; }

        private bool _isSelected;
        public bool isSelected
        {
            get
            {
                return _isSelected;
            }
            set
            {
                if (_isSelected != value && PropertyChanged != null)
                {
                    _isSelected = value;
                    NotifyPropertyChange("isSelected");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        private void NotifyPropertyChange(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    public enum NotifyType
    {
        StatusMessage,
        ErrorMessage
    };

    public enum ResultCodeType
    {
        操作成功 = 1000,//SUCCEED
        服务器内部错误=1100,//INTERNAL_ERROR
        授权失败 = 1200,//AUTH_FAILURE
        无权限 = 1300,//NO_AUTH
        请求参数错误 =1400,//BAD_REQUEST
        请求未发现 =404,//Not_Found
    }
}
