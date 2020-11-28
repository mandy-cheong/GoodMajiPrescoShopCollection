using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Transactions;
//using System.Data.OracleClient;
//↑須另外加入System.Data.OracleClient參考，才能using此命名空間，不過通常很少用OracleClient，所以把它註解掉


/// <summary>
/// 此類別為抽象類別，不允許被new，在使用時直接調用即可
/// </summary>
public abstract class SqlHelper
{


    /// <summary>
    /// SqlHelper預設資料庫連線字串
    /// </summary>
    public static readonly string connectionString = ConfigurationManager.ConnectionStrings["dbConn"].ConnectionString.Trim();
    //private static readonly string connectionString = "Data Source=WIN-5L90RF5C148;Initial Catalog=goodmaji_index;User ID=howard;Password=dqtmwxatvnmm";

    #region 資料提供者
    /// <summary>
    /// 資料提供者，依據目標資料庫不同，須修改不同類型的DbProviderFactory
    /// </summary>
    private static readonly DbProviderFactory dbProviderFactory = SqlClientFactory.Instance;
    //private static readonly DbProviderFactory dbProviderFactory = OdbcFactory.Instance;
    //private static readonly DbProviderFactory dbProviderFactory = OleDbFactory.Instance;
    //private static readonly DbProviderFactory dbProviderFactory = OracleClientFactory.Instance;
    #endregion


    /// <summary>
    /// 為執行命令準備參數
    /// </summary>
    /// <param name="cmd">DbCommand 命令</param>
    /// <param name="conn">資料庫連線</param>
    /// <param name="trans">交易處理</param>
    /// <param name="cmdType">DbCommand類型 (CommandType.StoredProcedure或CommandType.Text)</param>
    /// <param name="cmdText">DbCommand的T-SQL语句 例如：Select * from Products</param>
    /// <param name="cmdParms">使用到的參數集合</param>
    private static void PrepareCommand(DbCommand cmd, DbConnection conn, DbTransaction trans, CommandType cmdType, string cmdText,
        List<DbParameter> cmdParms)
    {
        //判斷資料庫連線狀態
        if (conn.State != ConnectionState.Open) { conn.Open(); }
        //判斷是否需要交易處理
        if (trans != null) { cmd.Transaction = trans; }
        cmd.Connection = conn;
        cmd.CommandText = cmdText;
        cmd.CommandType = cmdType;
        if (cmdParms != null && cmdParms.Count > 0)
        {
            foreach (DbParameter param in cmdParms)
            {
                //param.DbType = DbType.AnsiString;
                cmd.Parameters.Add(param);
            }
        }
    }

    #region 連線資料庫存取

    #region ExecuteNonQuery 異動資料

    /// <summary>
    /// 執行新增、修改、刪除指令，透過指定連接字串。
    /// </summary>
    /// <param name="connectionString">工程師自行指定DB連線字串</param>
    /// <param name="cmdType">DbCommand類型 (CommandType.StoredProcedure或CommandType.Text)</param>
    /// <param name="cmdText">預存程式名稱 或 T-SQL 语句</param>
    /// <param name="commandParameters">使用到的參數集合</param>
    /// <returns>受影響的資料筆數</returns>
    public static int ExecuteNonQuery(string connectionString, CommandType cmdType, string cmdText, List<DbParameter> commandParameters)
    {
        using (DbConnection conn = dbProviderFactory.CreateConnection())
        {
            conn.ConnectionString = connectionString;
            DbCommand cmd = conn.CreateCommand();
            //通過PrePareCommand方法將參數逐個加入到DbCommand的参數集合中
            PrepareCommand(cmd, conn, null, cmdType, cmdText, commandParameters);
            int val = cmd.ExecuteNonQuery();
            //清空SqlCommand中的参数列表
            cmd.Parameters.Clear();
            return val;

        }//end using 

    }

    /// <summary>
    /// 執行新增、修改、刪除指令，使用SqlHelper預設連接字串。
    /// </summary>
    /// <param name="cmdType">DbCommand類型 (CommandType.StoredProcedure或CommandType.Text)</param>
    /// <param name="cmdText">預存程式名稱 或 T-SQL 语句</param>
    /// <param name="commandParameters">使用到的參數集合</param>
    /// <returns>受影響的資料筆數</returns>
    public static int ExecuteNonQuery(CommandType cmdType, string cmdText, List<DbParameter> commandParameters)
    {
        return ExecuteNonQuery(connectionString, cmdType, cmdText, commandParameters);
    }

    /// <summary>
    /// 執行新增、修改、刪除指令，使用SqlHelper預設連接字串、CommandType.Text
    /// </summary>
    /// <param name="cmdText">CommandType.Text的T-Sql語句</param>
    /// <param name="commandParameters">使用到的參數集合</param>
    /// <returns>受影響的資料筆數</returns>
    public static int ExecuteNonQueryText(string cmdText, List<DbParameter> commandParameters)
    {
        return ExecuteNonQuery(CommandType.Text, cmdText, commandParameters);
    }

    #endregion

    #region  ExecuteScalar

    /// <summary>
    /// 取得第一行第一列的資料，通常使用在Select Count(*) From TableName 有聚合函數的Select指令
    /// 工程師自行指定DB連線字串
    /// </summary>
    /// <param name="connectionString">工程師自行指定DB連線字串</param>
    /// <param name="cmdType">DbCommand類型 (CommandType.StoredProcedure或CommandType.Text)</param>
    /// <param name="cmdText">預存程式名稱 或 T-SQL 語句</param>
    /// <param name="commandParameters">使用到的參數集合</param>
    /// <returns>回傳第一行第一列的資料，型別不確定</returns>
    public static object ExecuteScalar(string connectionString, CommandType cmdType, string cmdText, List<DbParameter> commandParameters)
    {
        using (DbConnection conn = dbProviderFactory.CreateConnection())
        {
            conn.ConnectionString = connectionString;//連線字串
            DbCommand cmd = conn.CreateCommand();
            PrepareCommand(cmd, conn, null, cmdType, cmdText, commandParameters);
            object val = cmd.ExecuteScalar();
            cmd.Parameters.Clear();//清除參數集合
            conn.Close();
            return val;
        }
    }
    /// <summary>
    /// 取得第一行第一列的資料，通常使用在Select Count(*) From TableName 有聚合函數的Select指令
    /// 使用SqlHelper預設DB連線字串
    /// </summary>
    /// <param name="cmdType">DbCommand類型 (CommandType.StoredProcedure或CommandType.Text)</param>
    /// <param name="cmdText">預存程式名稱 或 T-SQL 語句</param>
    /// <param name="commandParameters">使用到的參數集合</param>
    /// <returns>回傳第一行第一列的資料，型別不確定</returns>
    public static object ExecuteScalar(CommandType cmdType, string cmdText, List<DbParameter> commandParameters)
    {
        return ExecuteScalar(connectionString, cmdType, cmdText, commandParameters);
    }


    /// <summary>
    /// 取得第一行第一列的資料，通常使用在Select Count(*) From TableName 有聚合函數的Select指令
    /// 使用SqlHelper預設DB連線字串、CommandType.Text
    /// </summary>
    /// <param name="cmdText">T-SQL 語句</param>
    /// <param name="commandParameters">使用到的參數集合</param>
    /// <returns>回傳第一行第一列的資料，型別不確定</returns>
    public static object ExecuteScalarText(string cmdText, List<DbParameter> commandParameters)
    {
        return ExecuteScalar(connectionString, CommandType.Text, cmdText, commandParameters);
    }

    #endregion

    #region ExecuteReader 
    /// <summary>
    /// 執行Select查詢指令，工程師指定連線字串。
    /// 前端呼叫時記得要用using包住回傳的DbDataReader變數來關閉連線
    /// </summary>
    /// <param name="connectionString">工程師自行指定DB連線字串</param>
    /// <param name="cmdType">DbCommand類型 (CommandType.StoredProcedure或CommandType.Text)</param>
    /// <param name="cmdText">預存程式名稱 或 T-SQL 語句</param>
    /// <param name="commandParameters">使用到的參數集合</param>
    /// <returns>回傳DbDataReader指標</returns>
    public static DbDataReader ExecuteReader(string connectionString, CommandType cmdType, string cmdText, List<DbParameter> commandParameters)
    {
        using (DbConnection conn = dbProviderFactory.CreateConnection())
        {
            conn.ConnectionString = connectionString;//連線字串
            DbCommand cmd = conn.CreateCommand();
            //↓不寫這行的話，由實作的Provider決定數值，OleDb、Odbc、SqlClient預設30秒，OracleClient為0不逾時
            cmd.CommandTimeout = 0;//執行SQL指令時間，0為不逾時
            try
            {
                //開啟連線
                PrepareCommand(cmd, conn, null, cmdType, cmdText, commandParameters);
                DbDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                cmd.Parameters.Clear();//清除參數集合
                return reader;
            }
            catch (DbException ex)
            {
                conn.Close();//發生查詢例外就關閉連線
                throw ex;
            }
        }
    }

    /// <summary>
    /// 執行Select查詢指令，使用SqlHelper預設連線字串。
    /// 前端呼叫時記得要用using包住回傳的DbDataReader變數來關閉連線
    /// </summary>
    /// <param name="cmdType">DbCommand類型 (CommandType.StoredProcedure或CommandType.Text)</param>
    /// <param name="cmdText">預存程式名稱 或 T-SQL 語句</param>
    /// <param name="commandParameters">使用到的參數集合</param>
    /// <returns>回傳DbDataReader指標</returns>
    public static DbDataReader ExecuteReader(CommandType cmdType, string cmdText, List<DbParameter> commandParameters)
    {
        return ExecuteReader(connectionString, cmdType, cmdText, commandParameters);
    }

    /// <summary>
    /// 執行Select查詢指令，使用SqlHelper預設連線字串、CommandType.Text
    /// 前端呼叫時記得要用using包住回傳的DbDataReader變數來關閉連線
    /// </summary>
    /// <param name="cmdText">T-SQL 語句</param>
    /// <param name="commandParameters">使用到的參數集合</param>
    /// <returns>回傳DbDataReader指標</returns>
    public static DbDataReader ExecuteReaderText(string cmdText, List<DbParameter> commandParameters)
    {
        return ExecuteReader(CommandType.Text, cmdText, commandParameters);
    }

    #endregion

    #region 交易
    /// <summary>
    /// 執行多條SQL語句，實現資料庫交易，使用SqlHelper預設DB連線字串
    /// </summary>
    /// <param name="sqlStringList">參數集合</param>
    public static bool ExecuteSqlTran(List<Tuple<string, List<DbParameter>, CommandType>> sqlStringList)
    {
        return ExecuteSqlTran(connectionString, sqlStringList);
    }

    /// <summary>
    /// 執行多條SQL語句，實現資料庫交易
    /// </summary>
    /// <param name="sqlStringList">多條SQL語句</param>  
    public static bool ExecuteSqlTran(string connectionString, List<Tuple<string, List<DbParameter>, CommandType>> sqlStringList)
    {

        using (DbConnection conn = dbProviderFactory.CreateConnection())
        {
            bool rval = true;
            conn.ConnectionString = connectionString;//指定連線字串
            conn.Open();//開啟連線
            DbTransaction trans = conn.BeginTransaction();//開始交易
            DbCommand cmd = conn.CreateCommand();
            try
            {
                foreach (Tuple<string, List<DbParameter>, CommandType> item in sqlStringList)
                {
                    PrepareCommand(cmd, conn, trans, item.Item3, item.Item1, item.Item2);
                    cmd.ExecuteNonQuery();//執行一筆SQL異動語句
                                          //清空DbCommand中的參數集合
                    cmd.Parameters.Clear();
                }//end foreach
                trans.Commit();//交易提交
            }
            catch (DbException ex)
            {
                rval = false;
                trans.Rollback();//交易Rollback
                throw ex;
            }
            return rval;
        }
    }

    #endregion

    #endregion


    #region 

    /// <summary>
    /// 大量批次新增(限對象為Sql Server)，使用SqlHelper的預設連線
    /// </summary>
    /// <param name="dtSource">資料來源的DataTable</param>
    /// <param name="destDataTableName">目標資料庫的表格名稱</param>
    /// <param name="optionSqlBulk">匯入時的選項</param>
    public static void SqlBulkCopyFromDataTable(DataTable dtSource, string destDBTableName, SqlBulkCopyOptions optionSqlBulk = SqlBulkCopyOptions.Default)
    {
        SqlBulkCopyFromDataTable(connectionString, dtSource, destDBTableName, optionSqlBulk);

    }

    /// <summary>
    /// 大量批次新增(限對象為Sql Server)，工程師自行指定DB連線
    /// </summary>
    /// <param name="connectionString">DB連線字串</param>
    /// <param name="dtSource">資料來源的DataTable</param>
    /// <param name="destDataTableName">目標資料庫的表格名稱</param>
    /// <param name="optionSqlBulk">匯入時的選項</param>
    public static void SqlBulkCopyFromDataTable(string connectionString, DataTable dtSource, string destDBTableName, SqlBulkCopyOptions optionSqlBulk = SqlBulkCopyOptions.Default)
    {

        if (string.IsNullOrEmpty(destDBTableName))
        {
            throw new Exception("缺少目標資料庫的表格名稱");
        }

        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            conn.Open();//開啟連線
                        //開始交易
            SqlTransaction tran = conn.BeginTransaction();
            //宣告SqlBulkCopy  
            using (SqlBulkCopy sqlBC = new SqlBulkCopy(conn, optionSqlBulk, tran))
            {
                //設定一個批次量寫入多少筆資料       
                sqlBC.BatchSize = 1000;
                //設定逾時的秒數        
                sqlBC.BulkCopyTimeout = 30;
                //設定要寫入的資料表          
                sqlBC.DestinationTableName = destDBTableName;
                foreach (DataColumn dataCol in dtSource.Columns)
                {
                    //對應資料行         
                    sqlBC.ColumnMappings.Add(dataCol.ColumnName, dataCol.ColumnName);
                }//end foreach
                 //開始寫入新增 
                try
                {
                    sqlBC.WriteToServer(dtSource);
                    tran.Commit();//交易提交
                }
                catch (SqlException ex)
                {
                    tran.Rollback();//交易Rollback
                    throw ex;
                }

            }//end using 
        }//end using 

    }
    #endregion



    #region 離線資料庫存取

    #region GetDataTable

    public static DataSet GetDateSet(List<Tuple<string, List<DbParameter>>> sqlStringList, string connecttionString = null)
    {
        DataSet ds = new DataSet();
        using (DbConnection conn = dbProviderFactory.CreateConnection())
        {
            if (connecttionString != null)
                conn.ConnectionString = connecttionString;//指定連線字串
            else
                conn.ConnectionString = connectionString; //預設連線字串
            conn.Open();//開啟連線
            DbTransaction trans = conn.BeginTransaction();//開始交易
            DbCommand cmd = conn.CreateCommand();
            try
            {
                foreach (Tuple<string, List<DbParameter>> item in sqlStringList)
                {
                    DataTable dt = new DataTable();
                    PrepareCommand(cmd, conn, trans, CommandType.Text, item.Item1, item.Item2);
                    DbDataAdapter adapter = dbProviderFactory.CreateDataAdapter();//DbDataAdapter自己會開/關DB連線
                    adapter.SelectCommand = cmd;
                    adapter.Fill(dt);
                    ds.Tables.Add(dt);
                    cmd.Parameters.Clear();
                }//end foreach
                trans.Commit();//交易提交
            }
            catch (DbException ex)
            {
                trans.Rollback();//交易Rollback
                throw ex;
            }
        }
        return ds;
    }

    /// <summary>
    /// 取得Select指令回傳的結果集，工程師自行指定DB連線字串
    /// </summary>
    /// <param name="connecttionString">DB連線字串</param>
    /// <param name="cmdTye">DbCommand類型 (CommandType.StoredProcedure或CommandType.Text)</param>
    /// <param name="cmdText">預存程式名稱 或 T-SQL 语句</param>
    /// <param name="commandParameters">使用到的參數集合</param>
    /// <returns>取得Select指令回傳的結果集，型別DataTable</returns>
    public static DataTable GetDataTable(string connecttionString, CommandType cmdTye, string cmdText, List<DbParameter> commandParameters)
    {

        DataTable dt = new DataTable();
        using (DbConnection conn = dbProviderFactory.CreateConnection())
        {
            conn.ConnectionString = connectionString;//連線字串
            DbCommand cmd = conn.CreateCommand();
            //↓不寫這行的話，由實作的Provider決定數值，OleDb、Odbc、SqlClient預設30秒，OracleClient為0不逾時
            cmd.CommandTimeout = 0;//執行SQL指令時間，0為不逾時
            PrepareCommand(cmd, conn, null, cmdTye, cmdText, commandParameters);
            DbDataAdapter adapter = dbProviderFactory.CreateDataAdapter();//DbDataAdapter自己會開/關DB連線
            adapter.SelectCommand = cmd;
            adapter.Fill(dt);
            cmd.Parameters.Clear();
            conn.Close();//自關連線
        }
        return dt;
    }

    /// <summary>
    /// 取得Select指令回傳的結果集，使用SqlHelper預設DB連線字串
    /// </summary>
    /// <param name="cmdTye">DbCommand類型 (CommandType.StoredProcedure或CommandType.Text)</param>
    /// <param name="cmdText">預存程式名稱 或 T-SQL 语句</param>
    /// <param name="commandParameters">使用到的參數集合</param>
    /// <returns>取得Select指令回傳的結果集，型別DataTable</returns>
    public static DataTable GetDataTable(CommandType cmdTye, string cmdText, List<DbParameter> commandParameters)
    {
        return GetDataTable(connectionString, cmdTye, cmdText, commandParameters);
    }

    /// <summary>
    /// 取得Select指令回傳的結果集，使用SqlHelper預設DB連線字串、CommandType.Text
    /// </summary>
    /// <param name="cmdText">T-SQL 语句</param>
    /// <param name="commandParameters">使用到的參數集合</param>
    /// <returns>取得Select指令回傳的結果集，型別DataTable</returns>
    public static DataTable GetDataTableText(string cmdText, List<DbParameter> commandParameters = null)
    {
        return GetDataTable(CommandType.Text, cmdText, commandParameters);
    }

    /// <summary>
    /// 執行 SQL語法中 Select語法的方法(使用DataTable)。
    /// </summary>
    /// <param name="sqlCom">傳入SqlCommend</param>
    /// <returns>查詢到的DataTable</returns>
    public static DataTable queryBySql(SqlCommand sqlCom, String conStr = "")
    {
        if (conStr.Equals(""))
            conStr = connectionString;
        using (SqlConnection con = new SqlConnection(conStr))
        {
            con.Open();
            sqlCom.Connection = con;
            SqlDataAdapter l_adapter = new SqlDataAdapter(sqlCom);
            DataTable dt = new DataTable();
            try
            {
                l_adapter.Fill(dt);
            }
            catch (Exception e)
            {
                con.Close();
            }
            finally
            {
                l_adapter.Dispose();
                //關閉資料庫連線
                sqlCom.Dispose();
                con.Close();
            }
            return dt;
        }
    }
    /// <summary>
    /// 執行 SQL語法中 Update Insert Delete語法的方法。
    /// </summary>
    /// <param name="p_strSql">傳入Update Insert Delete語法</param>
    public static bool executeNonQry(SqlCommand sqlCom, String conStr = "")
    {
        if (conStr.Equals(""))
            conStr = connectionString;
        using (SqlConnection con = new SqlConnection(conStr))
        {
            CommittableTransaction ct = new CommittableTransaction();
            con.Open();
            con.EnlistTransaction(ct);
            //設定SqlCommand物件屬性
            bool isOK = true;
            sqlCom.Connection = con;
            sqlCom.CommandTimeout = 120;
            try
            {
                //執行Update Insert Delete語法。
                sqlCom.ExecuteNonQuery();
                ct.Commit();
            }
            catch (Exception ex)
            {
                ct.Rollback();
                con.Close();
                isOK = false;
            }
            finally
            {
                sqlCom.Dispose();
                //關閉資料庫連線
                con.Close();
            }
            return isOK;
        }
    }
    #endregion
    #endregion
}
