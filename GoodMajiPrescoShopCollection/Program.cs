﻿using GoodMajiPrescoShopCollection.Core.Implement;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace goodmaji
{
    class Program
    {
        static void Main(string[] args)
        {
            var service = new PrescoService();
            var emailService = new EmailService();

            var data = service.GetShopCollection("HK");
            int errrorCount = 0;
            int maxRetry = 3;

            while (!data.RStatus && errrorCount < maxRetry)
            {
                errrorCount++;
                data = service.GetShopCollection("HK");
            }

            if (errrorCount == maxRetry)
            {
                emailService.SendEmailAysnc("mandycheong@hawooo.com", "there is an error while sending api to presco shop collect  , error message :" + data.RMsg, "Presco Shop Collect Error");
                return;
            }


            try
            {

                var shopcollectDailyFac = new ShopCollectDailyFac();
                var shopcollectDaily = MapShopCollectDaily(data.DVal);
                shopcollectDailyFac.insertShopCollectDaily(shopcollectDaily);
            }
            catch (Exception ex)
            {
                var msg = ex.Message + "Stack Trace : " + ex.StackTrace;
                emailService.SendEmailAysnc("mandycheong@hawooo.com", "there is an error while sending api to presco shop collect  , error message :" + msg, "Presco Shop Collect Error");
            }



            Console.WriteLine(data);

        }

        private static bool IsNewShopCollectTime(ShopCollectTime shopCollectTime, DataTable existingShopCollectTime)
        {
            return !existingShopCollectTime.AsEnumerable().Any(x => x.Field<string>("SCT02") == shopCollectTime.SCT02
            && x.Field<string>("SCT03") == shopCollectTime.SCT03
            && x.Field<int>("SCT04") == shopCollectTime.SCT04);
        }
        private static List<ShopCollectTime> GetNewShopCollectTime(List<ShopCollectTime> shopCollectTimeList)
        {
            var newList = new List<ShopCollectTime>();
            var shopcollectTimeFac = new ShopCollectTimeFac();
            var existingShopCollectTime = shopcollectTimeFac.getShopCollectTime();
            foreach (var shopcollectTime in shopCollectTimeList)
            {
                if (IsNewShopCollectTime(shopcollectTime, existingShopCollectTime))
                    newList.Add(shopcollectTime);
            }
            return newList;
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
                    shopcollectdaily.SCD04 = item.ShortName;
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
                        shopcollectdaily.SCT04 = time.DayofWeek + 1;
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
