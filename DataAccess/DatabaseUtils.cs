using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Dongle.Utilities;
using System.Configuration;

namespace DataAccess
{
    public static class DatabaseUtils
    {
        /// <summary>
        /// 枚举：数据库类型
        /// </summary>
        public enum DatabaseType
        {
            ORACLE, SQLSERVER, SQLITE, MYSQL, ACCESS
        }

        //数据库连接串
        private static string connectionString;// = "Data Source=" + System.Environment.CurrentDirectory + "\\ProductTest.db3";
        private static IDbHelper _iDbHelper;        //数据库访问实例

        /// <summary>
        /// 静态构造方法（在第一次调用静态成员时调用，且只调用一次）
        /// </summary>
        static DatabaseUtils()
        {
            try
            {
                //读取配置文件数据库类型
                string strDbType = ConfigurationManager.AppSettings["DatabaseType"];
                //保存数据库类型（保存为枚举，不区分大小写）
                DatabaseType dbType = (DatabaseType)Enum.Parse(typeof(DatabaseType), strDbType, true);
                //创建对应数据库类型的实例
                _iDbHelper = createDbInstance(dbType);

                //连接字符串
                if (string.IsNullOrEmpty(connectionString))
                {
                    connectionString = ConfigurationManager.ConnectionStrings["ProductTest"].ConnectionString;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(System.Reflection.MethodBase.GetCurrentMethod(), "数据库访问配置异常:" + ex.Message);
            }
        }

        /// <summary>
        /// 创建实例（初始化接口对象）
        /// </summary>
        private static IDbHelper createDbInstance(DatabaseType dbType)
        {
            IDbHelper iDbHelperTemp = null;
            switch (dbType)
            {
                case DatabaseType.ORACLE:
                    //iDbHelperTemp = new OracleHelper();
                    break;
                case DatabaseType.SQLSERVER:
                    iDbHelperTemp = new SqlServerHelper();
                    break;
                case DatabaseType.SQLITE:
                    iDbHelperTemp = new SqliteHelper();
                    break;
                case DatabaseType.MYSQL:
                    iDbHelperTemp = new MySqlHelper();
                    break;
                case DatabaseType.ACCESS:
                    iDbHelperTemp = new AccessHelper();
                    break;
            }
            return iDbHelperTemp;
        }

        #region === 数据库执行方法 ===
        /// <summary>
        /// 执行 Transact-SQL 增删改语句并返回受影响的行数。
        /// </summary>
        public static int ExecuteNonQuery(string cmdText)
        {
            if (_iDbHelper == null || string.IsNullOrEmpty(connectionString))
            {
                return -1;
            }
            return _iDbHelper.ExecuteNonQuery(connectionString, cmdText);
        }

        /// <summary>
        /// 执行查询，返回DataTable
        /// </summary>
        public static DataTable GetDataTable(string cmdText)
        {
            if (_iDbHelper == null || string.IsNullOrEmpty(connectionString))
            {
                return null;
            }
            return _iDbHelper.GetDataTable(connectionString, cmdText);
        }
        #endregion

    }
}
