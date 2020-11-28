using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for OrderRequest
/// </summary>
public class OrderRequest
{
    public OrderRequest()
    {
        //
        // TODO: Add constructor logic here
        //
        Package = new List<Package>();
    }
    public string  ShipNo { get; set; }
    public int PageCount { get; set; }
    public string OrderNo { get; set; }
    public string  Collection { get; set; }
    public string ReceiverCountry { get; set; }
    public string ReceiverName { get; set; }
    public string ReceiverTel1 { get; set; }
    public string ReceiverTel2 { get; set; }
    public string ReceiverArea { get; set; }
    public string  ReceiverCode { get; set; }
    public string ReceiverAddr { get; set; }
    public string ReceiverEmail { get; set; }
    public decimal Weight { get; set; }
    public string Currency { get; set; }
    public string Clearance { get; set; }
    public string  Type { get; set; }
    public string Service { get; set; }
    public string Pay { get; set; }
    public string SenderName { get; set; }
    public string SenderAddr { get; set; }
    public string SenderTel1 { get; set; }
    public string SenderTel2 { get; set; }
    public string SenderCountry { get; set; }
    public string SenderEmail { get; set; }
    public string  SenderArea { get; set; }
    public string SenderCode { get; set; }
    public string Note { get; set; }
    public decimal Length { get; set; }
    public decimal Width { get; set; }
    public decimal  Height { get; set; }
    public string CodCurrency { get; set; }
    public string CodAmount { get; set; }
    public string DeliveryType { get; set; }
    public string RecipentDate { get; set; }
    public string ReciepentNote { get; set; }
    public string ParentId { get; set; }
    public string EshopId { get; set; }

    public bool IsMember { get { return ReceiverName == "Member"; } }

    public bool LightWeight { get { return Weight >=500; } }

    public List<Package> Package { get; set; }



}


public class ShipInfo
{
    public string MemberId { get; set; }
    public int DefaultCompany { get; set; }
    public int LockShipCompany { get; set; }
    public string Country { get; set; }
    public decimal Cod { get; set; }
    public string Type { get; set; }
    public string Clearance { get; set; }
    public string ShortName { get; set; }
    public string Postcode { get; set; }
    public decimal Weight { get; set; }
    public decimal VolWeight { get; set; }
    public bool CheckOutScanMember { get; set; }
    public bool CheckLockShipCompany { get; set; }
    public bool CheckEastMy { get; set; }
    public bool CheckMutiplePackage { get; set; }

    public bool CheckTransitMember { get {
            string[] noTransitMembers = { "52673436", "25100462" };
            return noTransitMembers.Contains(MemberId);
        }
    }

    public bool CheckClearance { get { return Clearance.Equals("不報關"); } }
}