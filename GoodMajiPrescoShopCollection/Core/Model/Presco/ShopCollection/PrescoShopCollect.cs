using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace goodmaji 
{
   public class PrescoShopCollect
    {
        public string CountryId { get; set; }
        public string CountryName { get; set; }
        public List<ShopInfo> data { get; set; }
      
    }

    public class OpeningHour
    {
        public int DayofWeek { get; set; }
        public string EndTime { get; set; }
        public string StartTime { get; set; }
    }

    public class ShopInfo
    {
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public object Directions { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int? MaxParcelStayDuration { get; set; }
        public string Name { get; set; }
        public string PartnerId { get; set; }
        public string PartnerName { get; set; }
        public string PostalCode { get; set; }
        public string ShortName { get; set; }
        public string Type { get; set; }
        public string State { get; set; }
        public string Area { get; set; }
        public List<OpeningHour> OpeningHours { get; set; }
    }
}
