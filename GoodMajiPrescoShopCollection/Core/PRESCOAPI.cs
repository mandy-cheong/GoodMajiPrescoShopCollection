using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;

namespace goodmaji
{
    /// <summary>
    /// PRESCOAPI 的摘要描述
    /// </summary>
    public class PRESCOAPI
    {
        //測試
        // public string _url = "https://test-cbec.sp88.tw";
        //正式
        //public string _url = "https://cbec.sp88.tw";
        public string _url = System.Configuration.ConfigurationManager.AppSettings["prescourl"];
        public PRESCOAPI()
        {

        }

        public class Account
        {
            public string parentId { get; set; }
            public string eshopId { get; set; }
            public string password { get; set; }
        }

        public class TokenInfo
        {
            public string token { get; set; }
            public string expiredTime { get; set; }
        }

        public class RootObject
        {
            public string ShipNo { get; set; }
            public string Label { get; set; }
        }


        public class MainShipment
        {
            public string ShipNo { get; set; }
            public string Collection { get; set; }
            public int PageCount { get; set; }
            public string SenderCountry { get; set; }
            public string SenderName { get; set; }
            public string SenderAddr { get; set; }
            public string SenderTel1 { get; set; }
            public object SenderTel2 { get; set; }
            public string ReceiverCountry { get; set; }
            public string ReceiverName { get; set; }
            public string ReceiverAddr { get; set; }
            public string ReceiverTel1 { get; set; }
            public object ReceiverTel2 { get; set; }
            public object MerchantId { get; set; }
            public string CountryCode { get; set; }
            public List<string> PackageItems { get; set; }
            public string BarCodeImg { get; set; }
        }

        public class LocalShipment
        {
            public string ShipNo { get; set; }
            public string Collection { get; set; }
            public int PageCount { get; set; }
            public string SenderCountry { get; set; }
            public string SenderName { get; set; }
            public string SenderAddr { get; set; }
            public string SenderTel1 { get; set; }
            public object SenderTel2 { get; set; }
            public string ReceiverCountry { get; set; }
            public string ReceiverName { get; set; }
            public string ReceiverAddr { get; set; }
            public string ReceiverTel1 { get; set; }
            public object ReceiverTel2 { get; set; }
            public string MerchantId { get; set; }
            public string CountryCode { get; set; }
            public List<object> PackageItems { get; set; }
            public string BarCodeImg { get; set; }
        }

        public class ShipInfo
        {
            public string ShipNo { get; set; }
            public MainShipment MainShipment { get; set; }
            public LocalShipment LocalShipment { get; set; }
        }
     

        public Dictionary<string, string> GetImgDic(List<string> shipNoList)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            string url = _url + "/api/shipment/labels/photo";
            string queryString = "?";
            foreach (string shipNo in shipNoList)
            {
                queryString += "shipNo=" + shipNo + "&";
            }
            queryString = queryString.Remove(queryString.Length - 1, 1);
            url = url + queryString;
            //測試用
            //shipNoList.Add("GMJI7115085743001");
            //shipNoList.Add("GMJI7115085742001");
            string labelInfo = Get(url);
            List<RootObject> rti = JsonConvert.DeserializeObject<List<RootObject>>(labelInfo);
            for (int i = 0; i < rti.Count; i++)
            {
                dic.Add(shipNoList[i], rti[i].Label);
            }
            return dic;
        }

        public List<ShipInfo> GetShipInfo(string shipNo)
        {
            List<ShipInfo> ListInfo = new List<ShipInfo>();
            string url = _url + "/api/shipment/labels?shipNo=" + shipNo;
            try
            {
                ListInfo = JsonConvert.DeserializeObject<List<ShipInfo>>(Get(url));
            }
            catch
            {

            }
            return ListInfo;
        }


        public string PostToken()
        {
            string msg = "";
            string url = _url + "/api/auth";
            Account ac = new Account
            {
                parentId = "7M1",
                eshopId = "000",
                //測試
                //password = "wJlxyJEDMWhmyAKASLcp",
                //正式
                //password = "2ttU8T3Ic6JLkMXxRelX",
                password = System.Configuration.ConfigurationManager.AppSettings["prescopass"]
            };
            var accountJSON = JsonConvert.SerializeObject(ac);
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";
            httpWebRequest.Timeout = 30000;
            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                streamWriter.Write(accountJSON);
                streamWriter.Flush();
                streamWriter.Close();
            }
            try
            {
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                    msg = result;
                }
            }
            catch (WebException wex)
            {
                msg = wex.ToString();
            }

            TokenInfo ti = JsonConvert.DeserializeObject<TokenInfo>(msg);
            msg = ti.token;
            return msg;
        }



        public string Get(string url, string token = "")
        {
            string msg = "";
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.Method = "GET";
            httpWebRequest.Timeout = 30000;
            
            if (token == "")
                token = PostToken();
            httpWebRequest.Headers.Add("Authorization", token);

            HttpWebResponse response = null;
            try
            {
                //若網站回應錯誤，就會在此引發WebException
                response = (HttpWebResponse)httpWebRequest.GetResponse();
            }
            catch (WebException e)
            {
                //網站回應錯誤,
                response = (HttpWebResponse)e.Response;
            }
            if (response != null)
            {
                using (Stream stream = response.GetResponseStream())
                {
                    using (StreamReader sr = new StreamReader(stream))
                    {
                        string result = sr.ReadToEnd();
                        switch (response.StatusCode)
                        {
                            case HttpStatusCode.OK:
                                msg = result;
                                break;
                            case HttpStatusCode.BadRequest:
                                msg = "400 bad request";
                                break;
                            default:
                                msg = "other Status";
                                break;
                        }
                        response.Close();
                    }
                }
            }
            return msg;
        }


          
        //1.取得配號
        //2.取得店取短名
        //3.建立統一數網提單
        //4.取得貨態
        //5.確認店取交貨單格式
    }
}