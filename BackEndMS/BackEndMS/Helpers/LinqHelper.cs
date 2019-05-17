using BackEndMS.Controls;
using BackEndMS.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;


namespace BackEndMS.Helpers
{
    public static class LinqHelper
    {
        //public const string pageQueryUri = "http://10.169.8.208:2330/bms/api/v1/PageQuery";
        //public const string blockQueryUri = "http://10.169.8.208:2330/bms/api/v1/blockQuery";
        // "https://bms.mmais.com.cn/api/v1/"; //"http://10.169.8.208:2330/bms/api/v1/";
        // public static string baseUri { get; set; }
        public static string token { get; set; }
        public static string RequestId { get; set; }
        /// <summary>
        /// 当前token是否有效
        /// </summary>
        public static bool isValid
        {
            get { return !String.IsNullOrEmpty(token); }
        }
        public static ObservableCollection<T> ToObservableCollectionAsync<T>(this IEnumerable<T> coll)
        {
            var c = new ObservableCollection<T>();
            try
            {
                foreach (var e in coll) c.Add(e);
            }
            catch (Exception ex)
            {
                MainPage.ShowErrorMessage(ex.Message);
            }
            return c;
        }
        public static T JsonDeserializer<T>(this string jsonString)
        {
            try
            {
                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));
                MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonString));
                T obj = (T)ser.ReadObject(ms);
                return obj;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public static void CopyPropertiesTo<T, TU>(this T source, TU dest)
        {
            var sourceProps = typeof(T).GetProperties().Where(x => x.CanRead).ToList();
            var destProps = typeof(TU).GetProperties()
                    .Where(x => x.CanWrite)
                    .ToList();

            foreach (var sourceProp in sourceProps)
            {
                if (destProps.Any(x => x.Name == sourceProp.Name))
                {
                    var p = destProps.First(x => x.Name == sourceProp.Name);
                    if (p.CanWrite)
                    { // check if the property can be set or no.
                        p.SetValue(dest, sourceProp.GetValue(source, null), null);
                    }
                }

            }

        }
        /// <summary>
        /// 根据查询类获取满足条件的集合
        /// </summary>
        /// <param name="searchEntry"></param>
        /// <returns></returns>
        public async static Task<string> GetSearchData(SearchEntry searchEntry, string moduleName)
        {
            var tempSearch = searchEntry.SearchEntity;
            List<string> result = new List<string>()
            {
                String.Format("PageIndex={0}",  searchEntry.PageIndex),
                String.Format("PageSize={0}",  searchEntry.PageSize),
                searchEntry.StartDate.HasValue?String.Format("ModifyDate_start={0}", GetUnixTimeStamp(searchEntry.StartDate)):null,
                searchEntry.EndDate.HasValue?String.Format("ModifyDate_end={0}", GetUnixTimeStamp(searchEntry.EndDate)):null,
            };
            switch (moduleName)
            {
                case "PageQuery":
                    {
                        if ((int)searchEntry.Flag != 0)
                            result.Add(String.Format("Flag={0}", (int)searchEntry.Flag));
                        if (searchEntry.Status != SubmitStatus.全部)
                            result.Add(String.Format("Status={0}", (int)searchEntry.Status));
                        if (!String.IsNullOrEmpty(searchEntry.SearchEntity.RequestId))
                            result.Add(String.Format("RequestId={0}", searchEntry.SearchEntity.RequestId.Trim()));
                        if (!String.IsNullOrEmpty(searchEntry.SearchEntity.Keywords))
                            result.Add(String.Format("Keywords={0}", searchEntry.SearchEntity.Keywords.Trim()));
                        if (!String.IsNullOrEmpty(searchEntry.SearchEntity.Operator))
                            result.Add(String.Format("Operator={0}", searchEntry.SearchEntity.Operator.Trim()));
                        if (searchEntry.IsExpire.HasValue)
                            result.Add(String.Format("IsExpire={0}", searchEntry.IsExpire.Value));
                    }
                    break;
                case "BlockQuery":
                    {
                        if (!String.IsNullOrEmpty(searchEntry.SearchEntity.RequestId))
                            result.Add(String.Format("RequestId={0}", searchEntry.SearchEntity.RequestId.Trim()));
                        if (!String.IsNullOrEmpty(searchEntry.SearchEntity.Keywords))
                            result.Add(String.Format("Keywords={0}", searchEntry.SearchEntity.Keywords.Trim()));
                        if (!String.IsNullOrEmpty(searchEntry.SearchEntity.Operator))
                            result.Add(String.Format("Operator={0}", searchEntry.SearchEntity.Operator.Trim()));
                        if (!String.IsNullOrEmpty(searchEntry.SearchEntity.Class1))
                            result.Add(String.Format("Class1={0}", searchEntry.SearchEntity.Class1.Trim()));
                        if (!String.IsNullOrEmpty(searchEntry.SearchEntity.Class2))
                            result.Add(String.Format("Class2={0}", searchEntry.SearchEntity.Class2.Trim()));
                        if (!String.IsNullOrEmpty(searchEntry.SearchEntity.Class3))
                            result.Add(String.Format("Class3={0}", searchEntry.SearchEntity.Class3.Trim()));
                        var enable = Enum.GetName(typeof(Status), searchEntry.SearchEntity.Enable);
                        if (enable != null)
                            result.Add(String.Format("Enable={0}", enable));
                        var Strategy = Enum.GetName(typeof(Strategy), searchEntry.SearchEntity.Strategy);
                        if (Strategy != null)
                            result.Add(String.Format("Strategy={0}", Strategy));
                        var match = Enum.GetName(typeof(Match), searchEntry.SearchEntity.Match);
                        if (null != match)
                            result.Add(String.Format("Match={0}", match));
                        var Level = Enum.GetName(typeof(Level), searchEntry.SearchEntity.Level);
                        if (null != Level)
                            result.Add(String.Format("Level={0}", Level));
                    }
                    break;
                case "SearchQuery":
                    {
                        if (!String.IsNullOrEmpty(searchEntry.SearchEntity.Keywords))
                            result.Add(String.Format("Keywords={0}", searchEntry.SearchEntity.Keywords.Trim()));
                        if (!String.IsNullOrEmpty(searchEntry.SearchEntity.Operator))
                            result.Add(String.Format("Operator={0}", searchEntry.SearchEntity.Operator.Trim()));
                    }
                    break;
                default:
                    break;
            }

            var param = String.Join("&", result);
            if (searchEntry.ExportType.HasValue && searchEntry.ExportType.Value == ExportType.AllPages)
                return await GetAllQueryData(moduleName,param);
            else
                return await GetData(param, moduleName);
        }
        /// <summary>
        /// 获取表的所有数据记录 用于导出全部
        /// </summary>
        /// <param name="moduleName">webapi中 对应表的接口关键表示</param>
        /// <returns></returns>
        public async static Task<string> GetAllQueryData(string moduleName,string param="")
        {
            string result = string.Empty;
            using (var client = new HttpClient())
            {
                try
                {
                    string apiUri = App.RootBaseUri + moduleName + "/all?"+ param;
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    var response = await client.GetAsync(apiUri, HttpCompletionOption.ResponseContentRead);
                    result = await response.Content.ReadAsStringAsync();
                }
                catch (Exception ex)
                {
                    await MainPage.ShowErrorMessage(ex.Message);
                }
                return result;
            }
        }
        /// <summary>
        /// 根据参数串和模块名称获取搜索结果集
        /// </summary>
        /// <param name="param">参数串</param>
        /// <param name="moduleName">模块名称</param>
        /// <returns></returns>
        public async static Task<string> GetData(string param, string moduleName)
        {
            string result = string.Empty;
            using (var client = new HttpClient())
            {
                try
                {
                    string apiUri = App.RootBaseUri + moduleName + "?" + param;
                    var requestMessage = new HttpRequestMessage(HttpMethod.Get, apiUri);
                    requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    var response = await client.SendAsync(requestMessage, HttpCompletionOption.ResponseContentRead);
                    result = await response.Content.ReadAsStringAsync();
                }
                catch (Exception ex)
                {
                    await MainPage.ShowErrorMessage(ex.Message);
                }
                return result;
            }
        }
        public async static Task<string> SaveData(String jsonStr, string moduleName)
        {
            string result = string.Empty;
            using (var client = new HttpClient())
            {
                try
                {
                    var SaveUri = App.RootBaseUri + moduleName;
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    StringContent content = new StringContent(jsonStr, Encoding.UTF8, "application/json");
                    var response = await client.PostAsync(SaveUri, content);
                    result = await response.Content.ReadAsStringAsync();
                }
                catch (Exception ex)
                {
                    await MainPage.ShowErrorMessage(ex.Message);
                }
                return result;
            }
        }
        /// <summary>
        /// 批量保存数据到云端
        /// </summary>
        /// <param name="jsonArray">批量数据转json串</param>
        /// <param name="moduleName">模块名称</param>
        /// <returns></returns>
        public async static Task<string> SaveBatchData(String jsonArray, string moduleName)
        {
            string result = string.Empty;
            using (var client = new HttpClient())
            {
                try
                {
                    var batchUri = App.RootBaseUri + moduleName + "/batch";
                    //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    //StringContent content = new StringContent(jsonArray,Encoding.UTF8, "application/json");
                    //var response = await client.PostAsync(pageQueryUri, content);
                    var requestMessage = new HttpRequestMessage(HttpMethod.Post, batchUri);
                    requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    requestMessage.Content = new StringContent(jsonArray, Encoding.UTF8, "application/json");
                    var response = await client.SendAsync(requestMessage, HttpCompletionOption.ResponseContentRead);
                    result = await response.Content.ReadAsStringAsync();
                }
                catch (Exception ex)
                {
                    await MainPage.ShowErrorMessage(ex.Message);
                }
                return result;
            }
        }
        internal static async Task<string> SaveFileAsync(List<UploadImage> UploadImages, string UploadName)
        {
            string result = string.Empty;
            if (UploadImages != null && UploadImages.Count > 0)
            {
                using (HttpClient client = new HttpClient())
                {
                    try
                    {
                        string apiUri = App.RootBaseUri + "File";
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                        var multipartContent = new MultipartFormDataContent();
                        foreach (var image in UploadImages)
                        {
                            var randomStream = image.UplodFile.OpenReadAsync().AsTask().GetAwaiter().GetResult();
                            var streamContent = new StreamContent(randomStream.AsStreamForRead());
                            streamContent.Headers.Add("Content-Type", image.ContentType);
                            multipartContent.Add(streamContent, UploadName, image.ImageTitle);
                        }
                        Task.Delay(1000);
                        HttpResponseMessage response = await client.PostAsync(apiUri, multipartContent);
                        result = await response.Content.ReadAsStringAsync();
                    }
                    catch (Exception ex)
                    {
                        await MainPage.ShowErrorMessage(ex.Message);
                    }
                }
            }
            return result;
        }
        public async static Task<string> DeleteData(string moduleName, string id)
        {
            string result = string.Empty;
            using (var client = new HttpClient())
            {
                try
                {
                    string apiUri = App.RootBaseUri + moduleName + "/" + id;
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    var response = await client.DeleteAsync(apiUri);
                    result = await response.Content.ReadAsStringAsync();
                }
                catch (Exception ex)
                {
                    await MainPage.ShowErrorMessage(ex.Message);
                }
                return result;
            }
        }
        public async static Task<string> UpdateData<T>(string moduleName, T entity)
        {
            string result = string.Empty;
            using (var client = new HttpClient())
            {
                try
                {
                    var id = entity.GetType().GetProperty("Id").GetValue(entity).ToString();
                    if (String.IsNullOrEmpty(id))
                    {
                        return string.Empty;
                    }
                    var updateJson = JsonConvert.SerializeObject(entity);
                    string apiUri = App.RootBaseUri + moduleName + "/" + id;
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    StringContent content = new StringContent(updateJson, Encoding.UTF8, "application/json");
                    var response = await client.PutAsync(apiUri, content);
                    result = await response.Content.ReadAsStringAsync();
                }
                catch (Exception ex)
                {
                    await MainPage.ShowErrorMessage(ex.Message);
                }
                return result;
            }
        }

        public async static Task<string> GetAttachment(string id)
        {
            string result = string.Empty;
            using (var client = new HttpClient())
            {
                try
                {
                    string apiUri = App.RootBaseUri + "File/" + id;
                    var requestMessage = new HttpRequestMessage(HttpMethod.Get, apiUri);
                    requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    var response = await client.SendAsync(requestMessage, HttpCompletionOption.ResponseContentRead);
                    result = await response.Content.ReadAsStringAsync();
                }
                catch (Exception ex)
                {
                    await MainPage.ShowErrorMessage(ex.Message);
                }
                return result;
            }
        }

        public async static Task<string> GetBingWS(string keywords,int level,string type)
        {
            string result = string.Empty;
            using (var client = new HttpClient())
            {
                try
                {
                   // https://bms.mmais.com.cn/api/v1/BingWS?keywords=服务&level=2&type=as 
                    string apiUri = App.RootBaseUri +String.Format("BingWS?keywords={0}&level={1}&type={2}",keywords,level,type);
                    var requestMessage = new HttpRequestMessage(HttpMethod.Get, apiUri);
                    requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    var response = await client.SendAsync(requestMessage, HttpCompletionOption.ResponseContentRead);
                    result = await response.Content.ReadAsStringAsync();
                }
                catch (Exception ex)
                {
                    await MainPage.ShowErrorMessage(ex.Message);
                }
                return result;
            }
        }
        #region   登录注册接口
        /// <summary>
        /// 用户登录接口
        /// </summary>
        /// <param name="username">登录用户名</param>
        /// <param name="password">密码</param>
        /// <returns></returns>
        public async static Task<string> login(String username, string password)
        {
            string result = string.Empty;
            using (var client = new HttpClient())
            {
                try
                {
                    /*  登陆地址改为 ：
  http://bms.mmais.com.cn/api/v1/Account/SignIn 
                      注册地址为：
  http://bms.mmais.com.cn/api/v1/Account/SignUp
  */
                    client.DefaultRequestHeaders.Add("Username", username);
                    client.DefaultRequestHeaders.Add("Password", password);
                    var response = await client.PostAsync(App.RootBaseUri + "Account/SignIn", new StringContent("", Encoding.UTF8));
                    result = await response.Content.ReadAsStringAsync();
                }
                catch (Exception ex)
                {
                    throw (ex);
                    // await MainPage.ShowErrorMessage(ex.Message);
                }
                return result;
            }
        }
        public async static Task<string> Signin(string userinfo)
        {
            string result = string.Empty;
            using (var client = new HttpClient())
            {
                try
                {
                    var UserUrl = App.RootBaseUri + "Account/SignUp";
                    // client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    StringContent content = new StringContent(userinfo, Encoding.UTF8, "application/json");
                    var response = await client.PostAsync(UserUrl, content);
                    result = await response.Content.ReadAsStringAsync();
                }
                catch (Exception ex)
                {
                    await MainPage.ShowErrorMessage(ex.Message);
                }
                return result;
            }
        }
        #endregion
        /// <summary>
        /// 转换时间戳为本地时间
        /// </summary>
        /// <param name="unixTimestamp">unix时间戳</param>
        /// <returns></returns>
        public static DateTime GetDateTime(this long? unixTimestamp)
        {
            DateTime startTime = new DateTime(1970, 1, 1); // 当地时区
            DateTime dt = DateTime.Now;
            if (unixTimestamp.HasValue && unixTimestamp != 0)
                dt = startTime.AddSeconds(unixTimestamp ?? 0);
            return dt;
        }
        /// <summary>
        ///将本地时间转换为 unix时间戳
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static long GetUnixTimeStamp(this DateTime? dateTime)
        {
            DateTime startTime = new DateTime(1970, 1, 1); // 当地时区
            long dt = (long)((dateTime ?? DateTime.Now) - startTime).TotalSeconds;
            return dt;
        }

    }
}
