using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for Package
/// </summary>
public class Package
{
    public Package()
    {
        //
        // TODO: Add constructor logic here
        //
    }

    public string SkuNo { get; set; }
    public int Qty { get; set; }
    public decimal Price { get; set; }
    public string EnglishName { get; set; }
    public string ChineseName { get; set; }
    public string Brand { get; set; }
    public string Origin { get; set; }
    public decimal NetWeight { get; set; }
}