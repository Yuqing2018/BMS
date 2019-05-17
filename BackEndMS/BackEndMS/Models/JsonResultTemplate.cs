using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace BackEndMS.Models
{
    #region 分页显示查询结果
    public class JsonSearchResultTemplate<T>
    {
        public int ResultCode { get; set; }
        public DataTemplate<T> Data { get; set; }
        public string Message { get; set; }
    }
    public class DataTemplate<T>
    {
        public int TotalSize { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public IEnumerable<T> Data { get; set; }
    }
    #endregion
    #region 获取所有记录结果
    public class JsonResultTemplate<T>
    {
        public int ResultCode { get; set; }
        public IEnumerable<T> Data { get; set; }
        public string Message { get; set; }
    }
    #endregion
    #region 登录返回结果
    public class JsonLoginTemplate
    {
        public int ResultCode { get; set; }
        public LoginResult Data { get; set; }
        public string Message { get; set; }
    }
    public class LoginResult
    {
        public string Token { get; set; }
        public UserInfo Claims { get; set; }
    }
    #endregion
    #region 增删改查返回结果
    public class JsonChangedTemplate
    {
        public int ResultCode { get; set; }
        public Object Data { get; set; }
        public string Message { get; set; }
    }
    #endregion
}
