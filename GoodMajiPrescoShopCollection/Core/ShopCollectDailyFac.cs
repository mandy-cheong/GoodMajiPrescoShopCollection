using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Text;
using System.Data.SqlClient;

namespace goodmaji
{
    public class ShopCollectDailyFac
    {
        public class ShopCollectDailyFilter
        {
            public string ShortName { get; set; }
            public DateTime STime { get; set; }
            public DateTime ETime { get; set; }
            public string Country { get; set; }
            public string PostCode { get; set; }
        }

        public bool insertShopCollectDaily(ShopCollectDaily obj)
        {
            if (SqlDbmanager.ExecuteNonQry(SqlExtension.GetInsertSqlCmd("ShopCollectDaily", obj)))
                return true;
            return false;
        }

        public int insertShopCollectDaily(List<ShopCollectDaily> shopCollects)
        {
            var cmdList = new List<SqlCommand>();
            foreach (var shopcollect in shopCollects)
            {
                cmdList.Add(SqlExtension.GetInsertSqlCmd("ShopCollectDaily", shopcollect));
            }
            return SqlDbmanager.ExecuteNonQryMutiSqlCmd(cmdList);
        }
        public bool updateShopCollectDaily(ShopCollectDaily obj)
        {
            if (SqlDbmanager.ExecuteNonQry(SqlExtension.GetUpdateSqlCmd("ShopCollectDaily", obj, new List<string> { "SCD02" }, new List<string> { "SCD02=@SCD02" })))
                return true;
            return false;
        }
        public bool deleteShopCollectDaily(ShopCollectDaily obj)
        {
            if (SqlDbmanager.ExecuteNonQry(SqlExtension.GetDeleteSqlCmd("ShopCollectDaily", obj, new List<string> { "SCD02=@SCD02" })))
                return true;
            return false;
        }
        public DataTable getShopCollectDaily()
        {
            string strSql = "SELECT * FROM ShopCollectDaily";
            DataTable dt = SqlDbmanager.queryBySql(SqlExtension.GetSelectSqlCmdByParams(strSql));
            return dt;
        }
        public List<ShopCollectDaily> getShopCollectDaily(DataTable dt)
        {
            List<ShopCollectDaily> rval = SqlExtension.ToList<ShopCollectDaily>(dt) as List<ShopCollectDaily>;
            return rval;
        }

        public DataTable GetShopCollectDt(ShopCollectDailyFilter shopCollectDailyFilter)
        {
            SqlCommand cmd = new SqlCommand();

            string strSql = @"SELECT 
                                SCD03 AS '日期',
                                SCD04 AS '店取短名',
                                SCD05 AS '郵遞區號',
                                SCD06 AS '地址1',
                                SCD07 AS '地址2',
                                SCD08 AS '店家名稱',
                                SCD09 AS '包裹放置天數',
                                SCD10 AS '店取類型',
                                SCD13 AS '國家' 
                                FROM ShopCollectDaily 
                                WHERE SCD12=1 AND SCD03 BETWEEN @ST AND @ET AND SCD13=@CODE ";
            if (!string.IsNullOrEmpty(shopCollectDailyFilter.ShortName))
            {
                strSql += " AND SCD04=@SCD04";
                cmd.Parameters.Add(SafeSQL.CreateInputParam("SCD04", SqlDbType.VarChar, shopCollectDailyFilter.ShortName));
            }
            if (!string.IsNullOrEmpty(shopCollectDailyFilter.PostCode))
            {
                strSql += " AND SCD05=@SCD05";
                cmd.Parameters.Add(SafeSQL.CreateInputParam("SCD05", SqlDbType.VarChar, shopCollectDailyFilter.PostCode));
            }
            cmd.CommandText = strSql;
            cmd.Parameters.Add(SafeSQL.CreateInputParam("ST", SqlDbType.Date, shopCollectDailyFilter.STime.ToString("yyyy-MM-dd")));
            cmd.Parameters.Add(SafeSQL.CreateInputParam("ET", SqlDbType.Date, shopCollectDailyFilter.ETime.ToString("yyyy-MM-dd")));
            cmd.Parameters.Add(SafeSQL.CreateInputParam("CODE", SqlDbType.VarChar, shopCollectDailyFilter.Country));
            return SqlDbmanager.queryBySql(cmd, System.Web.Configuration.WebConfigurationManager.ConnectionStrings["NormalConn"].ConnectionString);
        }

        //CREATE PROCEDURE CUShopCollectDaily
        //(
        //   @SCD01 Int,
        //   @SCD02 UniqueIdentifier,
        //   @SCD03 Date,
        //   @SCD04 NVarChar(200),
        //   @SCD05 VarChar(20),
        //   @SCD06 NVarChar(200),
        //   @SCD07 NVarChar(200),
        //   @SCD08 NVarChar(200),
        //   @SCD09 Int,
        //   @SCD10 VarChar(30),
        //   @SCD11 DateTime,
        //   @SCD12 Int
        //)
        //AS
        //BEGIN
        //DECLARE @ID UniqueIdentifier
        //SELECT @ID=SCD02 FROM ShopCollectDaily WHERE SCD02=@SCD02
        //if (@ID is not null)
        //   UPDATE ShopCollectDaily SET 
        //   SCD03=@SCD03,
        //   SCD04=@SCD04,
        //   SCD05=@SCD05,
        //   SCD06=@SCD06,
        //   SCD07=@SCD07,
        //   SCD08=@SCD08,
        //   SCD09=@SCD09,
        //   SCD10=@SCD10,
        //   SCD11=@SCD11,
        //   SCD12=@SCD12
        //   WHERE SCD02=@ID
        //   else
        //   INSERT INTO ShopCollectDaily(
        //   SCD02,
        //   SCD03,
        //   SCD04,
        //   SCD05,
        //   SCD06,
        //   SCD07,
        //   SCD08,
        //   SCD09,
        //   SCD10,
        //   SCD11,
        //   SCD12
        //   ) VALUES (
        //   @SCD02,
        //   @SCD03,
        //   @SCD04,
        //   @SCD05,
        //   @SCD06,
        //   @SCD07,
        //   @SCD08,
        //   @SCD09,
        //   @SCD10,
        //   @SCD11,
        //   @SCD12
        //)
        //   return @ID
        //END

        //CREATE TABLE ShopCollectDaily(
        //SCD01 Int IDENTITY(1,1),
        //SCD02 UniqueIdentifier,
        //SCD03 Date,
        //SCD04 NVarChar(200),
        //SCD05 VarChar(20),
        //SCD06 NVarChar(200),
        //SCD07 NVarChar(200),
        //SCD08 NVarChar(200),
        //SCD09 Int,
        //SCD10 VarChar(30),
        //SCD11 DateTime,
        //SCD12 Int,
        //SCD13 VarChar(10),
        // PRIMARY KEY(SCD02)
        //)
    }
}
