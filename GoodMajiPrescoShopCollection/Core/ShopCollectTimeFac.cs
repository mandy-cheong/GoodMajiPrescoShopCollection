using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace goodmaji
{
    public class ShopCollectTimeFac
    {
        public bool insertShopCollectTime(ShopCollectTime obj)
        {
            if (SqlDbmanager.ExecuteNonQry(SqlExtension.GetInsertSqlCmd("ShopCollectTime", obj)))
                return true;
            return false;
        }
        public int insertShopCollectTime(List<ShopCollectTime> times)
        {
            var cmdList = new List<SqlCommand>();
            foreach (var time in times)
            {
                cmdList.Add(SqlExtension.GetInsertSqlCmd("ShopCollectTime", time));
            }
            return SqlDbmanager.ExecuteNonQryMutiSqlCmd(cmdList);
        }
        public bool updateShopCollectTime(ShopCollectTime obj)
        {
            if (SqlDbmanager.ExecuteNonQry(SqlExtension.GetUpdateSqlCmd("ShopCollectTime", obj, new List<string> { "SCT02", "SCT03", "SCT04" }, new List<string> { "SCT02=@SCT02", "SCT03=@SCT03", "SCT04=@SCT04" })))
                return true;
            return false;
        }
        public bool deleteShopCollectTime(ShopCollectTime obj)
        {
            if (SqlDbmanager.ExecuteNonQry(SqlExtension.GetDeleteSqlCmd("ShopCollectTime", obj, new List<string> { "SCT02=@SCT02", "SCT03=@SCT03", "SCT04=@SCT04" })))
                return true;
            return false;
        }
        public DataTable getShopCollectTime()
        {
            string strSql = "SELECT * FROM ShopCollectTime";
            DataTable dt = SqlDbmanager.queryBySql(SqlExtension.GetSelectSqlCmdByParams(strSql));
            return dt;
        }
        public List<ShopCollectTime> getShopCollectTime(DataTable dt)
        {
            List<ShopCollectTime> rval = SqlExtension.ToList<ShopCollectTime>(dt) as List<ShopCollectTime>;
            return rval;
        }
        //CREATE PROCEDURE CUShopCollectTime
        //(
        //   @SCT01 Int,
        //   @SCT02 NVarChar(50),
        //   @SCT03 NVarChar(150),
        //   @SCT04 Int,
        //   @SCT05 VarChar(10),
        //   @SCT06 VarChar(10)
        //)
        //AS
        //BEGIN
        //DECLARE @ID Int
        //SELECT @ID=SCT02 FROM ShopCollectTime WHERE SCT02=@SCT02
        //if (@ID is not null)
        //   UPDATE ShopCollectTime SET 
        //   SCT05=@SCT05,
        //   SCT06=@SCT06
        //   WHERE SCT02=@ID
        //   else
        //   INSERT INTO ShopCollectTime(
        //   SCT02,
        //   SCT03,
        //   SCT04,
        //   SCT05,
        //   SCT06
        //   ) VALUES (
        //   @SCT02,
        //   @SCT03,
        //   @SCT04,
        //   @SCT05,
        //   @SCT06
        //)
        //   return @ID
        //END

        //CREATE TABLE ShopCollectTime(
        //SCT01 Int IDENTITY(1,1),
        //SCT02 NVarChar(50),
        //SCT03 NVarChar(150),
        //SCT04 Int,
        //SCT05 VarChar(10),
        //SCT06 VarChar(10),
        // PRIMARY KEY(SCT02, SCT03, SCT04)
        //)
    }
}
