using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for rval
/// </summary>
public class RVal
{
    public RVal()
    {
        //
        // TODO: Add constructor logic here
        //


    }

	public bool RStatus { get; set; }

	/// <summary>
	/// 回傳訊息
	/// </summary>
	public string RMsg { get; set; }

	/// <summary>
	/// 回傳的動態物件
	/// </summary>
	public dynamic DVal { get; set; }
}