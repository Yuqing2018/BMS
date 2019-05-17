using BackEndMS.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
namespace BackEndMS.Models
{
    /// <summary>
    /// 页面巡检
    /// </summary>
    public class PageQuery
    {
        public PageQuery()
        {
            Querys = new ObservableCollection<PageQueryEntry>();
        }
        public string RequestId { get; set; }
        public ValidDays ValidDays { get; set; }
        public ObservableCollection<PageQueryEntry> Querys { get; set; }

        public int KeywordsCount
        {
            get
            {
                return Querys.Select(o => o.Keywords).Distinct().Count();
            }
        }

        public int UrlCount
        {
            get
            {
                return Querys.Where(o => o.Flag == QueryEntryFlag.Url).Count();
            }
        }

        public int QsCount
        {
            get
            {
                return Querys.Where(o => o.Flag == QueryEntryFlag.QS).Count();
            }
        }
        public int AsCount
        {
            get
            {
                return Querys.Where(o => o.Flag == QueryEntryFlag.AS).Count();
            }
        }
        public int PQsCount
        {
            get
            {
                return Querys.Where(o => o.Flag == QueryEntryFlag.PQS).Count();
            }
        }
        public int PIQsCount
        {
            get
            {
                return Querys.Where(o => o.Flag == QueryEntryFlag.PIQS).Count();
            }
        }
        public int DomainsCount
        {
            get
            {
                return Querys.Where(o => o.Flag == QueryEntryFlag.Domain).Count();
            }
        }
        public DateTime? CreateDate
        {
            get
            {
                return LinqHelper.GetDateTime(Querys.OrderBy(o => o.ModifyDate).FirstOrDefault()?.ModifyDate);
            }
        }

        public DateTime? ModifyDate
        {
            get
            {
                return LinqHelper.GetDateTime(Querys.OrderByDescending(o => o.ModifyDate).FirstOrDefault()?.ModifyDate);
            }
        }

        public List<ValidDays> ValidDaysList
        {
            get
            {
                return Enum.GetValues(typeof(ValidDays)).Cast<ValidDays>().ToList();
            }
        }
    }
}
