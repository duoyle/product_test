using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using MySql.Data.MySqlClient;

namespace DataAccess
{
    /// <summary>
    /// 数据库操作实现类（for MySql）
    /// </summary>
    public class MySqlHelper : IDbHelper
    {
        /// <summary>
        /// 执行 SQL 增删改语句并返回受影响的行数。
        /// </summary>
        public int ExecuteNonQuery(string strConn, string cmdText)
        {
            if (string.IsNullOrEmpty(strConn) || string.IsNullOrEmpty(cmdText)) return 0;
            using (MySqlConnection mySqlConn = new MySqlConnection(strConn))
            {
                mySqlConn.Open();
                MySqlCommand mySqlCmd = mySqlConn.CreateCommand();
                MySqlTransaction mySqlTran = mySqlConn.BeginTransaction();
                try
                {
                    mySqlCmd.CommandText = cmdText;
                    mySqlCmd.Transaction = mySqlTran;
                    int result = mySqlCmd.ExecuteNonQuery();
                    mySqlTran.Commit();//提交事务
                    return result;
                }
                catch (Exception exc)
                {
                    mySqlTran.Rollback();
                    throw exc;
                }
            }
        }

        /// <summary>
        /// 执行查询，返回DataTable
        /// </summary>
        public DataTable GetDataTable(string strConn, string cmdText)
        {
            if (string.IsNullOrEmpty(strConn) || string.IsNullOrEmpty(cmdText)) return null;
            using (MySqlConnection mySqlConn = new MySqlConnection(strConn))
            {
                using (MySqlCommand mySqlCmd = mySqlConn.CreateCommand())
                {
                    mySqlCmd.CommandText = cmdText;
                    using (MySqlDataAdapter msda = new MySqlDataAdapter(mySqlCmd))
                    {
                        DataTable dt = new DataTable();
                        try
                        {
                            msda.Fill(dt);
                            return dt;
                        }
                        catch (Exception exc)
                        {
                            throw exc;
                        }
                    }
                }
            }
        }

    }
}
