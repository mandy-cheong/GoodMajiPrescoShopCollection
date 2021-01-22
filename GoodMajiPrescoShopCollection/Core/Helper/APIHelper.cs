using goodmaji;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

/// <summary>
/// Summary description for APIHelper
/// </summary>
public class APIHelper
{
    public string Url { get; set; }
    public string ContentType { get; set; }
    public string Method { get; set; }
    public string RequestData { get; set; }
    public string ResponseData { get; set; }

    public APIHelper()
    {
    }
   
    public RVal GETApi()
    {
        HttpWebRequest request = GenerateRequest();
        HttpWebResponse response = null;
        request.Method = "GET";
        var rval = new RVal();
        try
        {
            response = (HttpWebResponse)request.GetResponse();          
        }
       
        catch (WebException e)
        {
            //網站回應錯誤,
            response = (HttpWebResponse)e.Response;
        }
       finally
        {
            rval = GetResponseMsg(response);
        }
        return rval;
    }

    public async Task<RVal> PostApiAsync()
    {
        var rval = new RVal();
        HttpResponseMessage res = null;
        try
        {
            var req = new HttpRequestMessage(HttpMethod.Post, Url)
            {
                Content = new StringContent(RequestData, Encoding.UTF8, "application/json")
            };
            var prescoAPI = new PRESCOAPI();
            req.Headers.Add("Authorization", prescoAPI.PostToken());
            var client = new HttpClient();
            res = await client.SendAsync(req);
            rval = await GetResponseMsg(res);

        }
        catch (Exception ex)
        {
         
        }
        finally
        {
        }
        return rval;

    }

    public RVal PostApi()
    {
        HttpWebRequest request = GenerateRequest();
        request.Method = "POST";
        HttpWebResponse response=null;
        var rval = new RVal();
        try
        {
            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                streamWriter.Write(RequestData);
                streamWriter.Flush();
                streamWriter.Close();
            }
            response = (HttpWebResponse)request.GetResponse();

        }

        catch (WebException e)
        {
            //網站回應錯誤,
            response = (HttpWebResponse)e.Response;
        }
        finally
        {
            rval = GetResponseMsg(response);
        }
        return rval;
    }

    public HttpWebRequest GenerateRequest()
    {
        var httpWebRequest = (HttpWebRequest)WebRequest.Create(Url);
        var prescoAPI = new PRESCOAPI();
        if (!string.IsNullOrEmpty(ContentType))
            httpWebRequest.ContentType = ContentType;
        httpWebRequest.Headers.Add("Authorization", prescoAPI.PostToken());

        return httpWebRequest;
    }

   

    public RVal GetResponseMsg(HttpWebResponse response)
    {
        using (var streamReader = new StreamReader(response.GetResponseStream()))
        {
            ResponseData = streamReader.ReadToEnd();
            switch (response.StatusCode)
            {
                case HttpStatusCode.OK:
                    return new RVal { RStatus = true, RMsg = ResponseData };
                case HttpStatusCode.BadRequest:
                    return new RVal { RStatus = false, RMsg = ResponseData };
                default:
                    return new RVal { RStatus = false, RMsg = ResponseData };
            }
        }

        
    }
    public async Task<RVal> GetResponseMsg(HttpResponseMessage response)
    {
        var result = await response.Content.ReadAsStringAsync();
        switch (response.StatusCode)
        {
            case HttpStatusCode.OK:
                return new RVal { RStatus = true, RMsg = result };
            case HttpStatusCode.BadRequest:
                return new RVal { RStatus = false, RMsg = result };
            default:
                return new RVal { RStatus = false, RMsg = result };
        }
    }

    public static void AddLog(string rmsg, string exStr)
    {
        try
        {
            StreamWriter sw;
            DateTime Date = DateTime.Now;
            var path = System.IO.Path.GetFullPath("log");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string fileName = DateTime.Now.ToString("yyyyMMdd") + ".txt";
            var filePath = path + "/" + fileName;
            FileInfo fi = new FileInfo(filePath);

            if (fi.Exists)
            {
                sw = File.AppendText(filePath);
            }
            else
            {
                File.Create(filePath).Close();
                sw = File.AppendText(filePath);
            }
            sw.WriteLine("*----------------------------------------------------------");
            sw.WriteLine("err_Time:" + Date.ToString("yyyy-MM-dd HH:mm:ss") + "");
            sw.WriteLine("err_Msg:" + rmsg + "");
            sw.WriteLine("err_ExStr:" + exStr);
            sw.WriteLine("----------------------------------------------------------*");
            sw.Flush();
            sw.Close();
        }
        finally
        {
        }
    }
}
