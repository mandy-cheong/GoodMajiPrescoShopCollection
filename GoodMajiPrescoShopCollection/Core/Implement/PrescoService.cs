using goodmaji;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

/// <summary>
/// Summary description for PrescoService
/// </summary>
public class PrescoService
{
    //測試
   // public string _url = "https://cbec-test.sp88.tw";
    //正式
    public string _url = "https://cbec.sp88.tw";
    private readonly APIHelper _apiHelper;

    
    public PrescoService()
    {
        //
        // TODO: Add constructor logic here
        //
        _apiHelper = new APIHelper();
    }


    public RVal CreateOrder(List<OrderRequest> requests)
    {
        CheckRemainingShipNumber();
        var shipnumner = GetShipmentNumber(requests.Count);

        for (int i = 0; i < requests.Count; i++)
            requests[i].ShipNo = shipnumner[i].Number;


        var rval = SubmitOrder(requests); ;
        if (rval.RStatus)
            UpdateShipNumber(shipnumner);

        return rval;
    }

    private RVal RequestShipNumber(ShipNumberRequest request)
    {
        var url = _url + "/api/shipment/numbers?CountryId=" + request.CountryId + "&ShipCount=" + request.ShipCount;
        var data = JsonConvert.SerializeObject(request);
        var helper = new APIHelper { Url = url, RequestData = data };
        var rval = helper.GETApi();
        if (rval.RStatus)
        {
            rval.DVal = JsonConvert.DeserializeObject<List<string>>(rval.RMsg);
        }

        AddLog(helper);
        return rval;
    }

    private PrescoAPILog MapAPILog(APIHelper helper)
    {
        return new PrescoAPILog
        {
            SysId = Guid.NewGuid(),
            CDate = DateTime.Now,
            URL = helper.Url,
            RequestData = helper.RequestData,
            ResponseData = helper.ResponseData
        };
    }

    private RVal SubmitOrder(List<OrderRequest> request)
    {
        var url = _url + "/api/shipment/new";

        var data = JsonConvert.SerializeObject(request);
        var helper = new APIHelper { Url = url, RequestData = data, ContentType = "application/json" };

        var rval = helper.PostApi();
        if (rval.RStatus == false)
            rval.RMsg = JsonConvert.DeserializeObject<PrescoResponse>(rval.RMsg).Message;
        else
            AddOrderLog(request, helper);

        return rval;
    }

    private int AddOrderLog(List<OrderRequest> request, APIHelper aPIHelper)
    {
        var cmdList = new List<SqlCommand>();
        var prescoAPILog = MapAPILog(aPIHelper);
        foreach (var item in request)
        {
            var orderlog = new PrescoOrderLog();
            orderlog.SysId = Guid.NewGuid();
            orderlog.PrescoAPILogID = prescoAPILog.SysId;
            orderlog.PrescoShipID = item.ShipNo;
            orderlog.GMShipID = item.OrderNo;
            cmdList.Add(SqlExtension.GetInsertSqlCmd("Prescoorderlog", orderlog));
        }
        cmdList.Add(SqlExtension.GetInsertSqlCmd("PrescoAPILog", prescoAPILog));
        var rval = SqlDbmanager.ExecuteNonQryMutiSqlCmd(cmdList);
        return rval;
    }


    private bool AddLog(APIHelper helper)
    {
        PrescoAPILog prescoAPILog = MapAPILog(helper);
        var cmd = SqlExtension.GetInsertSqlCmd("PrescoAPILog", prescoAPILog);
        return SqlHelper.executeNonQry(cmd);
    }

    public void CheckRemainingShipNumber()
    {
        if (CountShipmentNumber() <= 10)
        {
            var requestShipNumber = RequestShipNumber(new ShipNumberRequest { ShipCount = 2, CountryId = "HK" }).DVal;
            var addNumbers = MapShipNumber(requestShipNumber);
            AddShipNumber(addNumbers);
        }
    }
    private int AddShipNumber(List<ShipmentNumber> shipmentNumbers)
    {
        var cmdList = new List<SqlCommand>();
        foreach (var number in shipmentNumbers)
        {
            cmdList.Add(SqlExtension.GetInsertSqlCmd("PrescoShipment", number));

        }
        var rval = SqlDbmanager.ExecuteNonQryMutiSqlCmd(cmdList);
        return rval;
    }
    private int UpdateShipNumber(List<ShipmentNumber> shipmentNumbers)
    {
        var cmdlist = new List<SqlCommand>();
        foreach (var shipnumber in shipmentNumbers)
        {
            shipnumber.Status = (int)ShipNumberStatus.Used;
            shipnumber.UDate = DateTime.Now;
            cmdlist.Add(SqlExtension.GetUpdateSqlCmd("PrescoShipment", shipnumber, new List<string> { "Id" }, "Id=@Id"));
        }
        var rval = SqlDbmanager.ExecuteNonQryMutiSqlCmd(cmdlist);

        return rval;
    }

    private int CountShipmentNumber()
    {
        var sql = "SELECT  Count(1) FROM PrescoShipment WHERE Status =1  ";
        var rval = int.Parse(SqlHelper.ExecuteScalarText(sql, null).ToString());
        return rval;

    }
    private List<ShipmentNumber> GetShipmentNumber(int topcount)
    {

        var cmd = new SqlCommand { CommandText = "SELECT TOP " + topcount + " * FROM PrescoShipment WHERE Status =1 ORDER BY CDate " };
        var dt = SqlDbmanager.queryBySql(cmd);
        var result = new List<ShipmentNumber>();
        foreach (DataRow dr in dt.Rows)
        {
            var number = new ShipmentNumber();
            number.Id = int.Parse(dr["id"].ToString());
            number.Number = dr["Number"].ToString();
            result.Add(number);
        }
        return result;

    }
    private List<ShipmentNumber> MapShipNumber(List<string> numbers)
    {
        var shipnumbers = new List<ShipmentNumber>();
        foreach (var num in numbers)
        {
            shipnumbers.Add(new ShipmentNumber
            {
                Number = num,
                CDate = DateTime.Now,
                Status = (int)ShipNumberStatus.Active
            });
        }
        return shipnumbers;
    }
    public RVal GetShopCollection(string country)
    {
        var url = _url + "/api/GetAllCollections?CountryId="+country;

        var helper = new APIHelper { Url = url};

        var rval = helper.GETApi();

        if (rval.RStatus)
            rval.DVal = JsonConvert.DeserializeObject<List<PrescoShopCollect>>(rval.RMsg);

        return rval;
    }
    


}


