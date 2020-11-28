using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for ShipmentNumber
/// </summary>
public class ShipmentNumber
{
    public ShipmentNumber()
    {
        //
        // TODO: Add constructor logic here
        //
    }

    public int? Id { get; set; }
    public string  Number { get; set; }
    public DateTime? CDate { get; set; }
    public string CBy { get; set; }
    public DateTime? UDate { get; set; }
    public string UBy { get; set; }
    public int Status { get; set; }

  
}

public enum ShipNumberStatus
{
    Active = 1,
    Used = 2
}
