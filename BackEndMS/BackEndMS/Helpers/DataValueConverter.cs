using BackEndMS.Models;
using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using System.Linq;
using Windows.UI.Xaml.Media;

namespace BackEndMS.Helpers
{
    public class ScenarioBindingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            Scenario s = value as Scenario;
            return s.Title;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return true;
        }
    }
    public sealed class VisibleIfTrueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool && (bool)value)
            {
                return Visibility.Visible;
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return (value is Visibility && (Visibility)value == Visibility.Visible);
        }
    }

    public sealed class CollapseIfTrueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool && (bool)value)
            {
                return Visibility.Collapsed;
            }

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return (value is Visibility && (Visibility)value == Visibility.Collapsed);
        }
    }
    public class DateTimeBindingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {

            try
            {
                DateTime rsDate;
                if (value is long)
                    return (long)value != 0?GetDateTime((long)value).ToString("yyyy-MM-dd HH:mm:ss"):"";
                //if (null != value && value is DateTime)
                //    return (value as DateTime?).Value.ToString("yyyy-MM-dd hh:mm:ss");
                if (null != value && DateTime.TryParse(value.ToString(), out rsDate) && rsDate != null)
                {
                    return new DateTimeOffset(rsDate);
                }
                else
                    return null;
                  //  return new DateTimeOffset();
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        private DateTime GetDateTime(long unixTimestamp)
        {
            DateTime startTime = new DateTime(1970, 1, 1); // 当地时区
            DateTime dt = startTime.AddSeconds(unixTimestamp).AddHours(8);
            return dt;
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            DateTimeOffset? rsDate;
            if (value is DateTimeOffset?)
            {
                rsDate = value as DateTimeOffset?;
                if (rsDate.HasValue)
                    return rsDate.Value.DateTime;
                else
                    return DateTime.Now;
            }
            else
                return DateTime.Now;
        }
    }

    public class BooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value != null && value is bool)
            {
                return !((bool)value);
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
    public class ListToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        { 
            if (value != null &&  value is List<RoleType> )
            {
                return String.Join(";",(value as List<RoleType>));
            }
            return String.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
    public class HalfofValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is double)
                return (double)value / 2;
            else
                return 500;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class IntToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (null != value && value is bool)
            {
                var result = value as bool?;
                if (result.Value)
                    return Status.启用;
                else
                    return Status.停用;
            }
            else
                return Status.启用; 
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            var result = (int)value;
            if (0 == result)
            {
                return false;
            }
            else return true;
           // throw new NotImplementedException();
        }
    }

    public class BoolToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            String result = "停用";
            if (value != null && value is bool)
            {
                result = (value as bool?).Value? "启用": "停用";
            }
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class ArrayToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            String[] arrays = new string[] { };
            if (value is String[] && (value as String[]).Length > 0)
            {
                arrays = value as String[];
            }
            else if(value is List<String> && (value as List<String>).Count > 0)
            {
                arrays = (value as List<String>).ToArray();
            }
            var result = String.Join("\r", arrays);
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class ListToSelectedOneConverter : DependencyObject, IValueConverter
    {
        public string CategoryValue
        {
            get { return (string)GetValue(CategoryValueProperty); }
            set { SetValue(CategoryValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CategoryValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CategoryValueProperty =
            DependencyProperty.Register("CategoryValue", typeof(string), typeof(ListToSelectedOneConverter), new PropertyMetadata(""));


        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is List<BlockQueryCategory>)
            {
                var list = (value as List<BlockQueryCategory>);
                return list.Where(x => x.Name == CategoryValue).FirstOrDefault();
            }
            else
                return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class EnumToStringConverter:IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var result = Enum.GetName(value.GetType(), value);
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
    public class EnumToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is SensitiveStatus)
            {
                var result = (SensitiveStatus)value;
                return (result == SensitiveStatus.未处理 || result == SensitiveStatus.已获取 || result == SensitiveStatus.已处理);
            }
            else
                return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
    public class IsExpireToStingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            String result = "有效";
            if (value != null && value is bool)
            {
                result = (value as bool?).Value ? "已过期" : "有效";
            }
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
    public class BoolToColorConverter : IValueConverter
    {
        Windows.UI.Color red = Windows.UI.Color.FromArgb(255, 255, 0, 0);
        Windows.UI.Color green = Windows.UI.Color.FromArgb(0, 0, 255, 0);
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value != null && value is bool)
            {
                return (value as bool?).Value ? new SolidColorBrush(red) : new SolidColorBrush(green);
            }
            return new SolidColorBrush(green);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class StringLengthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is string)
                return (value as String).Length;
            else return 1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class DocumentToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
