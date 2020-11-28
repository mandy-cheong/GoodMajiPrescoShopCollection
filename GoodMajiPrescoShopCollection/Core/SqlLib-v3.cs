using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Transactions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace goodmaji
{
    public class SafeSQL
    {
        public static string Quote(string strData)
        {
            return string.Format("'{0}'", strData.Replace("'", "''"));
        }

        public static string QuoteLike(string strData)
        {
            return string.Format("'%{0}%'", strData.Replace("'", "''"));
        }

        public static string QuoteLikeRight(string strData)
        {
            return string.Format("'{0}%'", strData.Replace("'", "''"));
        }

        public static string QuoteLikeLeft(string strData)
        {
            return string.Format("'%{0}'", strData.Replace("'", "''"));
        }

        public static string NoQuote(string strData)
        {
            return strData.Replace("'", "''");
        }

        #region Parameter
        public static Parameter CreateSessionParameter(string paramName, TypeCode type, string sessionField)
        {
            Parameter parameter = new Parameter(paramName, type);
            object objValue = HttpContext.Current.Session[sessionField];
            if (objValue == null)
            {
                parameter.DefaultValue = "";
            }
            else
            {
                parameter.DefaultValue = objValue.ToString();
            }
            return parameter;
        }

        public static Parameter CreateControlParameter(string paramName, TypeCode type, string idControl, string propertyName, Control control)
        {
            Parameter parameter = new Parameter(paramName, type);
            Control component = control.FindControl(idControl);
            object objValue = DataBinder.Eval(component, propertyName);
            if (objValue == null)
            {
                parameter.DefaultValue = "";
            }
            else
            {
                parameter.DefaultValue = objValue.ToString();
            }
            return parameter;
        }
        #endregion Parameter

        #region SqlParameter

        public static SqlParameter CreateInputParam(string paramName, SqlDbType dbType, object objValue)
        {
            SqlParameter parameter = new SqlParameter(paramName, dbType);

            if (objValue == null || objValue == DBNull.Value)
            {
                parameter.IsNullable = true;
                parameter.Value = DBNull.Value;
            }
            else
            {
                if (dbType.Equals(SqlDbType.UniqueIdentifier))
                {
                    parameter.Value = System.Data.SqlTypes.SqlGuid.Parse(objValue.ToString());
                }
                else
                {
                    parameter.Value = objValue;
                }
            }
            return parameter;
        }

        public static SqlParameter CreateInputParam(string paramName, SqlDbType dbType, int size, object objValue)
        {
            SqlParameter parameter = CreateInputParam(paramName, dbType, objValue);
            parameter.Size = size;
            return parameter;
        }

        public static SqlParameter CreateOutputParam(string paramName, SqlDbType dbType, int size)
        {
            SqlParameter parameter = new SqlParameter(paramName, dbType);
            parameter.Direction = ParameterDirection.Output;
            parameter.Size = size;
            return parameter;
        }

        #endregion SqlParameter
    }

    public static class FieldCheck
    {
        public static bool isInt(string v)
        {
            int i = 0;
            if (int.TryParse(v, out i))
                return true;
            else
                return false;
        }

        public static bool isDecimal(string v)
        {
            decimal d = 0;
            if (decimal.TryParse(v, out d))
                return true;
            else
                return false;
        }

        public static bool isDouble(string v)
        {
            double d = 0;
            if (double.TryParse(v, out d))
                return true;
            else
                return false;
        }

        public static bool isFloat(string v)
        {
            float f = 0;
            if (float.TryParse(v, out f))
                return true;
            else
                return false;
        }

        public static bool isDateTime(string v)
        {
            DateTime d;
            if (DateTime.TryParse(v, out d))
                return true;
            else
                return false;
        }
    }

    public class PropertyVal
    {
        public PropertyVal() { }
        public PropertyVal(string name, Type type, string value)
        {
            this.pName = name;
            this.pType = type;
            this.pValue = value;
        }
        //Property Name
        public string pName { get; set; }
        //Property Type
        public Type pType { get; set; }
        //Property Value
        public string pValue { get; set; }
    }

    public static class SqlExtension
    {

        private static Dictionary<Type, SqlDbType> typeMap;

        static SqlExtension()
        {
            typeMap = new Dictionary<Type, SqlDbType>();
            typeMap[typeof(Guid)] = SqlDbType.UniqueIdentifier;
            typeMap[typeof(string)] = SqlDbType.NVarChar;
            typeMap[typeof(String)] = SqlDbType.NVarChar;
            typeMap[typeof(char[])] = SqlDbType.NVarChar;
            typeMap[typeof(byte)] = SqlDbType.TinyInt;
            typeMap[typeof(short)] = SqlDbType.SmallInt;
            typeMap[typeof(int)] = SqlDbType.Int;
            typeMap[typeof(long)] = SqlDbType.BigInt;
            typeMap[typeof(byte[])] = SqlDbType.Image;
            typeMap[typeof(bool)] = SqlDbType.Bit;
            typeMap[typeof(DateTime)] = SqlDbType.DateTime2;
            typeMap[typeof(DateTimeOffset)] = SqlDbType.DateTimeOffset;
            typeMap[typeof(decimal)] = SqlDbType.Money;
            typeMap[typeof(Decimal)] = SqlDbType.Money;
            typeMap[typeof(float)] = SqlDbType.Real;
            typeMap[typeof(double)] = SqlDbType.Float;
            typeMap[typeof(TimeSpan)] = SqlDbType.Time;
            typeMap[typeof(DataTable)] = SqlDbType.Structured;

        }

        public static SqlDbType GetDbType(Type giveType)
        {

            giveType = Nullable.GetUnderlyingType(giveType) ?? giveType;

            if (typeMap.ContainsKey(giveType))
            {
                return typeMap[giveType];
            }
            throw new ArgumentException(string.Format("{0} is not a supported .NET class", giveType.FullName));
        }


        #region DataTable Extension
        public static IList<T> ToList<T>(this DataTable table) where T : new()
        {
            IList<PropertyInfo> properties = typeof(T).GetProperties().ToList();
            IList<T> result = new List<T>();
            //取得DataTable所有的row data
            foreach (var row in table.Rows)
            {
                var item = MappingItem<T>((DataRow)row, properties);
                result.Add(item);
            }
            return result;
        }



        private static T MappingItem<T>(DataRow row, IList<PropertyInfo> properties) where T : new()
        {
            T item = new T();
            foreach (var property in properties)
            {
                if (row.Table.Columns.Contains(property.Name))
                {
                    //針對欄位的型態去轉換
                    if (property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        object convertedValue = System.Convert.ChangeType(row[property.Name], Nullable.GetUnderlyingType(property.PropertyType));
                        property.SetValue(item, convertedValue, null);
                        //property.SetValue(item, row[property.Name].ToString());
                    }
                    else if (property.PropertyType == typeof(DateTime))
                    {
                        DateTime dt = new DateTime();
                        if (DateTime.TryParse(row[property.Name].ToString(), out dt))
                        {
                            property.SetValue(item, dt, null);
                        }
                        else
                        {
                            property.SetValue(item, null, null);
                        }
                    }
                    else if (property.PropertyType == typeof(decimal))
                    {
                        decimal val = new decimal();
                        decimal.TryParse(row[property.Name].ToString(), out val);
                        property.SetValue(item, val, null);
                    }
                    else if (property.PropertyType == typeof(double))
                    {
                        double val = new double();
                        double.TryParse(row[property.Name].ToString(), out val);
                        property.SetValue(item, val, null);
                    }
                    else if (property.PropertyType == typeof(int))
                    {
                        int val = new int();
                        int.TryParse(row[property.Name].ToString(), out val);
                        property.SetValue(item, val, null);
                    }
                    else
                    {
                        if (row[property.Name] != DBNull.Value)
                        {
                            property.SetValue(item, row[property.Name].ToString(), null);
                        }
                    }
                }
            }
            return item;
        }
        #endregion

        #region properties
        /// <summary>
        /// 回傳非null的屬性
        /// </summary>
        /// <param name="pObject"></param>
        /// <returns>Return PropertyVal List</returns>
        public static List<PropertyVal> GetPropertiesVal(object pObject, bool excludeNull = true)
        {
            List<PropertyVal> propertyList = new List<PropertyVal>();
            if (pObject != null)
            {
                foreach (var prop in pObject.GetType().GetProperties())
                {
                    if (prop.GetValue(pObject, null) != null)
                    {
                        PropertyVal val = new PropertyVal();
                        val.pName = prop.Name;
                        val.pValue = prop.GetValue(pObject, null).ToString();
                        val.pType = prop.GetValue(pObject, null).GetType();
                        propertyList.Add(val);
                    }
                }
            }
            return propertyList;
        }


        public static SqlParameter TransProperty2Param(PropertyVal p)
        {
            SqlParameter param = new SqlParameter();
            param = SafeSQL.CreateInputParam(p.pName, GetDbType(p.pType), p.pValue);
            return param;
        }


        #endregion

        #region delete
        /// <summary>
        /// Get Delete String
        /// </summary>
        /// <param name="tName"></param>
        /// <param name="wStr"></param>
        /// <returns></returns>
        public static string GetDeleteStr(string tName, params string[] wStr)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("DELETE FROM " + tName);
            for (int i = 0; i <= wStr.Length - 1; i++)
            {
                if (i == 0)
                {
                    sb.Append(" WHERE " + wStr[i].ToString() + " ");
                }
                else
                {
                    sb.Append(" " + wStr[i].ToString() + " ");
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Get Delete SqlCommand
        /// </summary>
        /// <param name="tName"></param>
        /// <param name="wStr"></param>
        /// <returns></returns>
        public static SqlCommand GetDeleteSqlCmd(string tName, List<string> wStr, List<SqlParameter> paramList = null)
        {
            return GetSqlCmd(GetDeleteStr(tName, wStr.ToArray()), paramList);
        }

        /// <summary>
        /// Get Delete SqlCommand
        /// </summary>
        /// <param name="tName"></param>
        /// <param name="wStr"></param>
        /// <returns></returns>
        public static SqlCommand GetDeleteSqlCmd(string tName, object obj, List<string> wStr, List<SqlParameter> paramList = null)
        {
            SqlCommand cmd = new SqlCommand();
            cmd = GetSqlCmd(GetDeleteStr(tName, wStr.ToArray()), obj);
            if (paramList != null)
                cmd.Parameters.AddRange(paramList.ToArray());
            return cmd;
        }


        #endregion

        #region update
        /// <summary>
        /// Get Update String
        /// </summary>
        /// <param name="tName">資料表名稱</param>
        /// <param name="obj">傳入物件</param>
        /// <param name="wStr">Where string</param>
        /// <returns></returns>
        public static string GetUpdateStr(string tName, object obj, List<string> noUpStrList, params string[] wStr)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("UPDATE " + tName + " SET ");
            List<PropertyVal> _objList = GetPropertiesVal(obj);
            foreach (PropertyVal v in _objList)
            {
                if (noUpStrList == null)
                {
                    sb.Append(v.pName.ToString() + "=@" + v.pName.ToString() + ",");
                }
                else
                {
                    if (!noUpStrList.Contains(v.pName.ToString()))
                        sb.Append(v.pName.ToString() + "=@" + v.pName.ToString() + ",");
                }
            }
            sb.Remove(sb.ToString().Length - 1, 1);
            for (int i = 0; i <= wStr.Length - 1; i++)
            {
                if (i == 0)
                {
                    sb.Append(" WHERE " + wStr[i].ToString() + " ");
                }
                else
                {
                    sb.Append(" AND " + wStr[i].ToString() + " ");
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Get Update String
        /// </summary>
        /// <param name="tName">資料表名稱</param>
        /// <param name="wStr">Where string</param>
        /// <returns></returns>
        public static string GetUpdateStr(string tName, Dictionary<string, string> fList, string wStr)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("UPDATE " + tName + " SET ");
            foreach (KeyValuePair<string, string> val in fList)
            {
                sb.Append(val.Key + "=@" + val.Value + ",");
            }
            sb.Remove(sb.ToString().Length - 1, 1);
            if (wStr.Length > 0)
            {
                sb.Append(" WHERE ");
                sb.Append(wStr);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Get Update String
        /// </summary>
        /// <param name="tName">資料表名稱</param>
        /// <param name="obj"></param>
        /// <param name="wStr"></param>
        /// <returns></returns>
        public static SqlCommand GetUpdateSqlCmd(string tName, object obj, List<string> noUpStrList, List<string> wStr, List<SqlParameter> pList = null)
        {
            string sqlStr = GetUpdateStr(tName, obj, noUpStrList, wStr.ToArray());
            SqlCommand cmd = new SqlCommand();
            cmd = GetSqlCmd(sqlStr, obj);
            if (pList != null)
                cmd.Parameters.AddRange(pList.ToArray());
            return cmd;
        }

        public static SqlCommand GetUpdateSqlCmd(string tName, object obj, List<string> noUpStrList, string wStr, List<SqlParameter> pList = null)
        {
            string sqlStr = GetUpdateStr(tName, obj, noUpStrList, wStr);
            SqlCommand cmd = new SqlCommand();
            cmd = GetSqlCmd(sqlStr, obj);
            if (pList != null)
                cmd.Parameters.AddRange(pList.ToArray());
            return cmd;
        }


        #endregion

        /// <summary>
        /// Get Insert SqlCommand
        /// </summary>
        /// <param name="tName"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static SqlCommand GetInsertSqlCmd(string tName, object obj)
        {
            return GetSqlCmd(GetInsertStr(tName, obj), obj);
        }

        public static SqlCommand GetInsertSqlCmd(string tName, List<PropertyVal> pList)
        {
            return GetSqlCmd(GetInsertStr(tName, pList).ToString(), (object)pList.ToArray());
        }

        public static string GetInsertStr(string tName, List<string> fNameList)
        {
            string fStr = "";
            string fPStr = "";
            foreach (string s in fNameList)
            {
                fStr += s.ToString() + ",";
                fPStr += "@" + s.ToString() + ",";
            }
            string sqlStr = "INSERT INTO " + tName + " ( " + fStr.Trim(',').ToString() + " ) VALUES ( " + fPStr.Trim(',').ToString() + " )";
            return sqlStr;
        }

        #region insert 
        /// <summary>
        /// Get Insert String By PropertyVal
        /// </summary>
        /// <param name="tName"></param>
        /// <param name="newOB"></param>
        /// <returns></returns>
        public static string GetInsertStr(string tName, object obj)
        {
            List<PropertyVal> _objList = GetPropertiesVal(obj);
            string sqlStr = GetInsertStr(tName, _objList);
            return sqlStr;
        }

        public static string GetInsertStr(string tName, List<PropertyVal> pList)
        {
            string fStr = ""; //欄位字串
            string pStr = ""; //參數字串
            foreach (PropertyVal p in pList)
            {
                fStr += p.pName.ToString() + ",";
                pStr += "@" + p.pName.ToString() + ",";
            }
            fStr = fStr.Trim(',');
            pStr = pStr.Trim(',');
            StringBuilder sb = new StringBuilder();
            sb.Append("INSERT INTO " + tName + " (");
            sb.Append(fStr);
            sb.Append(") VALUES (");
            sb.Append(pStr);
            sb.Append(")");
            return sb.ToString();
        }


        #region select
        /// <summary>
        /// Get Select SqlCommand
        /// </summary>
        /// <param name="strSql">查詢字串</param>
        /// <param name="objList">物件陣列</param>
        /// <returns></returns>
        public static SqlCommand GetSelectSqlCmdByObjs(string strSql, params object[] objList)
        {
            SqlCommand cmd = new SqlCommand();
            foreach (object obj in objList)
            {
                cmd.Parameters.AddRange(TransObjToParamList(obj).ToArray());
            }
            cmd.CommandText = strSql;
            return cmd;
        }


        public static SqlCommand GetSelectSqlCmdByParams(string strSql, params SqlParameter[] paramList)
        {
            return GetSqlCmd(strSql, paramList);
        }

        #endregion

        #region SqlCommand Extension

        public static SqlCommand GetSqlCmd(string strSql)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = strSql;
            return cmd;
        }
        public static SqlCommand GetSqlCmd(string strSql, params SqlParameter[] paramList)
        {
            SqlCommand cmd = new SqlCommand();
            foreach (SqlParameter param in paramList)
            {
                cmd.Parameters.Add(param);
            }
            cmd.CommandText = strSql;
            return cmd;
        }
        public static SqlCommand GetSqlCmd(string strSql, params PropertyVal[] pList)
        {
            SqlCommand cmd = new SqlCommand();
            foreach (PropertyVal p in pList)
            {
                cmd.Parameters.Add(TransProperty2Param(p));
            }
            cmd.CommandText = strSql;
            return cmd;
        }
        public static SqlCommand GetSqlCmd(string strSql, object obj)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = strSql;
            List<PropertyVal> _objList = GetPropertiesVal(obj);
            foreach (PropertyVal p in _objList)
            {
                cmd.Parameters.Add(TransProperty2Param(p));
            }
            return cmd;
        }
        public static SqlCommand GetSqlCmd(string strSql, List<PropertyVal> pList)
        {
            return GetSqlCmd(strSql, (object)pList.ToArray());
        }


        /// <summary>
        /// Object Trans to SqlCommand
        /// </summary>
        /// <param name="strSQL">查詢字串</param>
        /// <param name="obj">物件陣列</param>
        /// <returns></returns>
        public static SqlCommand TransToSqlCommand(string strSQL, object obj)
        {
            return GetSqlCmd(strSQL, obj);
        }

        /// <summary>
        /// Trans to SqlCommand
        /// </summary>
        /// <param name="strSQL">Sql字串</param>
        /// <param name="parameters">Parameter參數陣列</param>
        /// <returns></returns>
        public static SqlCommand TransToSqlCommand(string strSQL, params SqlParameter[] parameters)
        {
            return GetSqlCmd(strSQL, parameters);
        }
        /// <summary>
        /// Object Trans to Param List
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static List<SqlParameter> TransObjToParamList(object obj)
        {
            List<SqlParameter> parmList = new List<SqlParameter>();
            List<PropertyVal> _objList = GetPropertiesVal(obj);
            foreach (PropertyVal p in _objList)
            {
                parmList.Add(TransProperty2Param(p));
            }
            return parmList;
        }

        #endregion
    }

    public static class SqlDbmanager
    {
        #region SqlErrorLog
        public static void AddErrorLog(SqlCommand sqlCmd, string exStr)
        {
            try
            {
                System.Web.HttpServerUtility Server = System.Web.HttpContext.Current.Server;
                string fileName = DateTime.Now.ToString("yyyyMMdd") + ".txt";
                string filePath = Server.MapPath("../log/") + fileName;


                DateTime Date = DateTime.Now;

                if (!Directory.Exists(HttpContext.Current.Server.MapPath("../log/")))
                {
                    Directory.CreateDirectory(HttpContext.Current.Server.MapPath("../log/"));
                }


                using (StreamWriter sw = File.AppendText(filePath))
                {
                    sw.WriteLine("*----------------------------------------------------------");
                    sw.WriteLine("err_Time:" + Date.ToString("yyyy-MM-dd HH:mm:ss") + "");
                    sw.WriteLine("err_Page:" + HttpContext.Current.Request.Url.AbsoluteUri);
                    sw.WriteLine("err_SqlStr:" + sqlCmd.CommandText + "");
                    sw.WriteLine("err_ComPars:");
                    for (int i = 0; i < sqlCmd.Parameters.Count; i++)
                    {
                        string pStr = "  " + sqlCmd.Parameters[i].ParameterName + ":" + sqlCmd.Parameters[i].Value + "";
                        sw.WriteLine(pStr);
                    }
                    sw.WriteLine("err_ExStr:" + exStr);
                    sw.WriteLine("----------------------------------------------------------*");
                    sw.Flush();
                    sw.Close();
                }

            }
            finally
            {
            }

        }
        public static void AddErrorLog(string strSql, string exStr)
        {
            try
            {
                StreamWriter sw;
                DateTime Date = DateTime.Now;
                if (!Directory.Exists(HttpContext.Current.Server.MapPath("../log/")))
                {
                    Directory.CreateDirectory(HttpContext.Current.Server.MapPath("../log/"));
                }
                System.Web.HttpServerUtility Server = System.Web.HttpContext.Current.Server;
                string fileName = DateTime.Now.ToString("yyyyMMdd") + ".txt";
                FileInfo fi = new FileInfo(Server.MapPath("../log" + "/" + fileName));

                if (fi.Exists)
                {
                    sw = File.AppendText(Server.MapPath("../log" + "/" + fileName));
                }
                else
                {
                    File.Create(Server.MapPath("../log") + "/" + fileName).Close();
                    sw = File.AppendText(Server.MapPath("../log" + "/" + fileName));
                }
                sw.WriteLine("*----------------------------------------------------------");
                sw.WriteLine("err_Time:" + Date.ToString("yyyy-MM-dd HH:mm:ss") + "");
                sw.WriteLine("err_SqlStr:" + strSql + "");
                sw.WriteLine("err_ExStr:" + exStr);
                sw.WriteLine("----------------------------------------------------------*");
                sw.Flush();
                sw.Close();
            }
            finally
            {
            }
        }
        #endregion

        #region ConnStr


        public static String DefaultConStr = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DefaultConn"].ConnectionString;
        #endregion

        #region Query
        /// <summary>
        /// 執行 SQL語法中 Select語法的方法(使用DataTable)。
        /// 
        /// 
        /// </summary>
        /// <param name="sqlStr">傳入Select語法</param>
        /// <returns>查詢到的DataTable</returns>
        public static DataTable queryBySql(String sqlStr, String conStr = "")
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = sqlStr;
            return queryBySql(cmd, conStr);
        }



        public static DataTable queryBySql(String sqlStr, string tableName, String conStr = "")
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = sqlStr;
            return queryBySql(cmd, tableName, conStr);
        }
        public static SqlDataReader excuteReturnDataReader(SqlCommand sqlCom, String conStr = "")
        {
            if (conStr.Equals(""))
                conStr = DefaultConStr;

            SqlConnection con = new SqlConnection(conStr);
            sqlCom.Connection = con;
            con.Open();
            SqlDataReader reader = sqlCom.ExecuteReader(CommandBehavior.CloseConnection);
            return reader;

        }
        public static IEnumerable<IDataRecord> excuteReaderReturnIEnumerable(SqlCommand sqlCom, String conStr = "")
        {
            if (conStr.Equals(""))
                conStr = DefaultConStr;
            using (SqlConnection con = new SqlConnection(conStr))
            {
                sqlCom.Connection = con;
                con.Open();
                using (var reader = sqlCom.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    while (reader.Read())
                    {
                        yield return (IDataRecord)reader;
                    }
                    yield return null;
                }
            }
        }

        /// <summary>
        /// 執行 SQL語法中 Select語法的方法(使用DataTable)。
        /// </summary>
        /// <param name="sqlCom">傳入SqlCommend</param>
        /// <returns>查詢到的DataTable</returns>
        public static DataTable queryBySql(SqlCommand sqlCom, string tableName, String conStr = "")
        {
            if (conStr.Equals(""))
                conStr = DefaultConStr;
            using (SqlConnection con = new SqlConnection(conStr))
            {
                con.Open();
                sqlCom.Connection = con;
                sqlCom.CommandTimeout = 120;
                SqlDataAdapter l_adapter = new SqlDataAdapter(sqlCom);
                DataTable dt = new DataTable(tableName);
                try
                {
                    l_adapter.Fill(dt);
                }
                catch (Exception e)
                {
                    AddErrorLog(sqlCom, e.ToString());
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
        /// 執行 SQL語法中 Select語法的方法(使用DataTable)。
        /// </summary>
        /// <param name="sqlCom">傳入SqlCommend</param>
        /// <returns>查詢到的DataTable</returns>
        public static DataTable queryBySql(SqlCommand sqlCom, String conStr = "")
        {
            if (conStr.Equals(""))
                conStr = DefaultConStr;
            using (SqlConnection con = new SqlConnection(conStr))
            {
                con.Open();
                sqlCom.Connection = con;
                sqlCom.CommandTimeout = 120;
                SqlDataAdapter l_adapter = new SqlDataAdapter(sqlCom);
                DataTable dt = new DataTable();
                try
                {
                    l_adapter.Fill(dt);
                }
                catch (Exception e)
                {
                    AddErrorLog(sqlCom, e.ToString());
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


        public static DataSet queryBySqlReturnDs(SqlCommand sqlCom, String conStr = "")
        {
            if (conStr.Equals(""))
                conStr = DefaultConStr;
            using (SqlConnection con = new SqlConnection(conStr))
            {
                con.Open();
                sqlCom.Connection = con;
                SqlDataAdapter l_adapter = new SqlDataAdapter(sqlCom);
                DataSet ds = new DataSet();
                try
                {
                    l_adapter.Fill(ds);
                }
                catch (Exception e)
                {
                    AddErrorLog(sqlCom, e.ToString());
                    con.Close();
                }
                finally
                {
                    l_adapter.Dispose();
                    //關閉資料庫連線
                    sqlCom.Dispose();
                    con.Close();
                }
                return ds;
            }
        }



        /// <summary>
        /// 執行 SQL語法中 Select語法的方法(使用DataTable)。
        /// </summary>
        /// <param name="cmdList">傳入List<SqlCommend></SqlCommend></param>
        /// <returns>查詢到的DataSet</returns>
        public static DataSet queryBySql(List<string> strList, String conStr = "")
        {
            List<SqlCommand> cmdList = new List<SqlCommand>();
            foreach (string s in strList)
            {
                SqlCommand cmd = new SqlCommand();
                cmd.CommandText = s;
                cmdList.Add(cmd);
            }
            return queryBySql(cmdList, conStr);
        }

        /// <summary>
        /// 執行 SQL語法中 Select語法的方法(使用DataTable)。
        /// </summary>
        /// <param name="cmdList">傳入List<SqlCommend></SqlCommend></param>
        /// <returns>查詢到的DataSet</returns>
        public static DataSet queryBySql(List<SqlCommand> cmdList, String conStr = "")
        {
            if (conStr.Equals(""))
                conStr = DefaultConStr;
            using (SqlConnection con = new SqlConnection(conStr))
            {
                SqlDataAdapter adp = new SqlDataAdapter();
                DataSet ds = new DataSet();
                try
                {
                    con.Open();
                    foreach (SqlCommand cmd in cmdList)
                    {
                        try
                        {
                            DataTable nDT = new DataTable();
                            cmd.Connection = con;
                            adp.SelectCommand = cmd;
                            adp.Fill(nDT);
                            ds.Tables.Add(nDT);
                        }
                        catch (Exception e)
                        {
                            AddErrorLog(cmd, e.ToString());
                            con.Close();
                        }
                    }
                }
                finally
                {
                    //關閉資料庫連線
                    con.Close();
                }
                return ds;
            }
        }
        #endregion

        #region execute
        /// <summary>
        /// 取回插入資料的編號
        /// </summary>
        /// <param name="sqlStr"></param>
        /// <returns></returns>
        public static Int32 ExecuteScalar(String sqlStr, String conStr = "")
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = sqlStr + ";SELECT SCOPE_IDENTITY()";
            return ExecuteScalar(cmd, conStr);
        }

        /// <summary>
        /// 取回插入資料的編號
        /// </summary>
        /// <param name="p_strSql"></param>
        /// <returns></returns>
        public static Int32 ExecuteScalar(SqlCommand sqlCom, String conStr = "")
        {
            if (conStr.Equals(""))
                conStr = DefaultConStr;
            using (SqlConnection con = new SqlConnection(conStr))
            {
                CommittableTransaction ct = new CommittableTransaction();
                con.Open();
                con.EnlistTransaction(ct);
                sqlCom.CommandText = sqlCom.CommandText + ";SELECT SCOPE_IDENTITY()";
                sqlCom.Connection = con;
                sqlCom.CommandTimeout = 120;
                int i = 0;
                try
                {
                    i = Convert.ToInt32(sqlCom.ExecuteScalar());
                    ct.Commit();
                }
                catch (Exception ex)
                {
                    AddErrorLog(sqlCom, ex.ToString());
                    ct.Rollback();
                    con.Close();
                    return -1;
                }
                finally
                {
                    sqlCom.Dispose();
                    con.Close();
                }
                return i;
            }
        }



        /// <summary>
        /// 執行 SQL語法中 Update Insert Delete語法的方法。
        /// </summary>
        /// <param name="sqlStr">傳入Update Insert Delete語法</param>
        public static bool ExecuteNonQry(String sqlStr, String conStr = "")
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = sqlStr;
            return ExecuteNonQry(cmd, conStr);
        }

        /// <summary>
        /// 執行 SQL語法中 Update Insert Delete語法的方法。
        /// </summary>
        /// <param name="p_strSql">傳入Update Insert Delete語法</param>
        public static bool ExecuteNonQry(SqlCommand sqlCom, String conStr = "")
        {
            if (conStr.Equals(""))
                conStr = DefaultConStr;
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
                    AddErrorLog(sqlCom, ex.ToString());
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


        /// <summary>
        /// 執行 SQL語法中 Update Insert Delete語法的方法。
        /// </summary>
        /// <param name="cmdList">傳入List<SqlCommend></SqlCommend></param>
        /// <returns>查詢到的DataSet</returns>
        public static int ExecuteNonQryMutiSqlCmd(List<SqlCommand> cmdList, String conStr = "")
        {
            if (conStr.Equals(""))
                conStr = DefaultConStr;
            int rval = 1;

            using (SqlConnection con = new SqlConnection(conStr))
            {
                CommittableTransaction ct = new CommittableTransaction();
                con.Open();
                con.EnlistTransaction(ct);
                try
                {
                    foreach (SqlCommand cmd in cmdList)
                    {
                        try
                        {
                            cmd.Connection = con;
                            cmd.ExecuteNonQuery();
                            cmd.Dispose();
                        }
                        catch (Exception ex)
                        {
                            AddErrorLog(cmd, ex.ToString());
                            ct.Rollback();
                            rval = -1;
                            con.Close();
                            return rval;
                        }
                    }
                    ct.Commit();
                }
                finally
                {
                    con.Close();
                }
            }

            return rval;
        }

        /// <summary>
        /// Excute主從表
        /// </summary>
        /// <param name="mcmd">Master Cmd</param>
        /// <param name="ccmdList">Detail Cmd (主資料表ID SET=@mid)</param>
        /// <param name="conStr">連線字串</param>
        /// <returns></returns>
        public static int ExcuteNonQryMasterDetail(SqlCommand mcmd, List<SqlCommand> ccmdList, String conStr = "")
        {
            if (conStr.Equals(""))
                conStr = DefaultConStr;
            int rval = -1;

            using (SqlConnection con = new SqlConnection(conStr))
            {
                CommittableTransaction ct = new CommittableTransaction();
                con.Open();
                con.EnlistTransaction(ct);
                try
                {
                    mcmd.CommandText = mcmd.CommandText + ";SELECT SCOPE_IDENTITY()";
                    mcmd.Connection = con;
                    mcmd.CommandTimeout = 120;
                    rval = Convert.ToInt32(mcmd.ExecuteScalar());
                    foreach (SqlCommand cmd in ccmdList)
                    {
                        try
                        {
                            //cmd.CommandText = cmd.CommandText.Replace("@mid", rval.ToString());
                            cmd.Parameters.Add(SafeSQL.CreateInputParam("@mid", SqlDbType.Int, rval));
                            cmd.Connection = con;
                            cmd.ExecuteNonQuery();
                            cmd.Dispose();
                        }
                        catch (Exception ex)
                        {
                            //addErrorLog(cmd, ex.ToString());
                            ct.Rollback();
                            rval = -1;
                            return rval;
                        }
                    }
                    ct.Commit();
                }
                finally
                {
                    mcmd.Dispose();
                    con.Close();
                }
            }

            return rval;
        }


        /// <summary>
        /// 執行 SQL語法中 Update Insert Delete語法的方法。
        /// </summary>
        /// <param name="cmdList">傳入List<SqlCommend></SqlCommend></param>
        /// <returns>查詢到的DataSet</returns>
        public static string ExecuteSqlBySpReturnStr(SqlCommand sqlCom, String conStr = "")
        {
            if (conStr.Equals(""))
                conStr = DefaultConStr;
            using (SqlConnection con = new SqlConnection(conStr))
            {
                CommittableTransaction ct = new CommittableTransaction();
                con.Open();
                con.EnlistTransaction(ct);
                //設定SqlCommand物件屬性
                sqlCom.Connection = con;
                sqlCom.CommandTimeout = 120;
                sqlCom.CommandType = CommandType.StoredProcedure;
                SqlParameter retValParam = sqlCom.Parameters.Add("@RETURN_VALUE", SqlDbType.VarChar, 250);
                retValParam.Direction = ParameterDirection.Output;
                try
                {
                    sqlCom.ExecuteNonQuery();
                    ct.Commit();
                }
                catch (Exception ex)
                {
                    AddErrorLog(sqlCom, ex.ToString());
                    ct.Rollback();
                    con.Close();
                }
                finally
                {
                    sqlCom.Dispose();
                    con.Close();
                }
                return retValParam.Value.ToString();
                //return retVal.Value.ToString();
            }
        }

        public static int ExecuteSqlBySpReturnInt(SqlCommand sqlCom, String conStr = "")
        {
            if (conStr.Equals(""))
                conStr = DefaultConStr;
            using (SqlConnection con = new SqlConnection(conStr))
            {

                CommittableTransaction ct = new CommittableTransaction();
                con.Open();
                con.EnlistTransaction(ct);
                //設定SqlCommand物件屬性
                sqlCom.Connection = con;
                sqlCom.CommandTimeout = 120;
                sqlCom.CommandType = CommandType.StoredProcedure;
                SqlParameter retValParam = sqlCom.Parameters.Add("@RETURN_VALUE", SqlDbType.VarChar, 250);
                retValParam.Direction = ParameterDirection.ReturnValue;
                try
                {
                    sqlCom.ExecuteNonQuery();
                    ct.Commit();
                }
                catch (Exception ex)
                {
                    AddErrorLog(sqlCom, ex.ToString());
                    ct.Rollback();
                    con.Close();
                }
                finally
                {
                    sqlCom.Dispose();
                    con.Close();
                }
                return Convert.ToInt32(retValParam.Value);
                //return retVal.Value.ToString();
            }
        }



        /// <summary>
        /// DataTable批次跟新
        /// </summary>
        /// <param name="dt">跟新的DataTable</param>
        /// <param name="conStr">連接字串</param>
        /// <returns></returns>
        public static bool ExcuteSqlBulkCopy(DataTable dt, String conStr = "")
        {
            if (conStr.Equals(""))
                conStr = DefaultConStr;
            using (SqlConnection con = new SqlConnection(conStr))
            {
                bool isOK = true;
                //設定SqlCommand物件屬性
                con.Open();
                SqlBulkCopy SBC = new SqlBulkCopy(con);
                SBC.BulkCopyTimeout = 300000;
                SBC.DestinationTableName = dt.TableName;
                foreach (DataColumn dc in dt.Columns)
                {
                    SBC.ColumnMappings.Add(dc.ColumnName, dc.ColumnName);
                }
                try
                {
                    SBC.WriteToServer(dt);
                }
                catch (Exception ex)
                {
                    isOK = false;
                    SBC.Close();
                    con.Close();
                }
                finally
                {
                    SBC.Close();
                    con.Close();
                }
                return isOK;
                //return retVal.Value.ToString();
            }
        }
        #endregion
    }
}


#endregion