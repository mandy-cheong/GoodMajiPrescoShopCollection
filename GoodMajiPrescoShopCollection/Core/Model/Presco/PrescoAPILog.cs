using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for PrescoAPILog
/// </summary>
public class PrescoAPILog
{
    public PrescoAPILog()
    {
        //
        // TODO: Add constructor logic here
        //
    }


    public string URL { get; set; }
    public string  RequestData { get; set; }
    public string ResponseData { get; set; }
    public DateTime CDate { get; set; }
}