using goodmaji;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace goodmaji
{
    class Program
    {
        static void Main(string[] args)
        {
            var service = new PrescoService();
            var data = service.GetShopCollection("HK");

            var shopcollectDailyFac = new ShopCollectDailyFac();
            var shopcollectDaily = MapShopCollectDaily(data.DVal);
            shopcollectDailyFac.insertShopCollectDaily(shopcollectDaily);

            var shopcollectTimeFac = new ShopCollectTimeFac();
            var shopCollectTime = MapShopCollectTime(data.DVal);
            shopcollectTimeFac.insertShopCollectTime(shopCollectTime);
            Console.WriteLine(data);
        }

        private static List<ShopCollectDaily> MapShopCollectDaily(List<PrescoShopCollect> shopCollectionResponses)
        {
            var result = new List<ShopCollectDaily>();
            foreach (var shopcollect in shopCollectionResponses)
            {
                foreach (var item in shopcollect.data)
                {
                    var shopcollectdaily = new ShopCollectDaily();
                    shopcollectdaily.SCD02 = Guid.NewGuid().ToString();
                    shopcollectdaily.SCD03 = DateTime.Now.ToString("yyyy-MM-dd");
                    shopcollectdaily.SCD04 = item.Name;
                    shopcollectdaily.SCD05 = item.PostalCode;
                    shopcollectdaily.SCD06 = item.Address1;
                    shopcollectdaily.SCD07 = item.Address2;
                    shopcollectdaily.SCD08 = item.Name;
                    shopcollectdaily.SCD09 = item.MaxParcelStayDuration;
                    shopcollectdaily.SCD10 = "SHOP";
                    shopcollectdaily.SCD11 = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"); ;
                    shopcollectdaily.SCD12 = 1;
                    shopcollectdaily.SCD13 = shopcollect.CountryId;

                    result.Add(shopcollectdaily);
                }
            }
            return result;
        }

        private static List<ShopCollectTime> MapShopCollectTime(List<PrescoShopCollect> shopCollectionResponses)
        {
            var result = new List<ShopCollectTime>();
            foreach (var shopcollect in shopCollectionResponses)
            {
                foreach (var item in shopcollect.data)
                {
                    foreach (var time in item.OpeningHours)
                    {
                        var shopcollectdaily = new ShopCollectTime();
                        shopcollectdaily.SCT02 = item.Name;
                        shopcollectdaily.SCT03 = item.ShortName;
                        shopcollectdaily.SCT04 = time.DayofWeek+1;
                        shopcollectdaily.SCT05 = time.StartTime;
                        shopcollectdaily.SCT06 = time.EndTime;
                        result.Add(shopcollectdaily);
                    }
                    
                }
            }
            return result;
        }
    }
}
