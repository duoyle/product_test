using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Data.Common;

namespace DataAccess
{
    /// <summary>
    /// 数据库操作实现类(for Sql Server)
    /// </summary>
    internal class SqlServerHelper : IDbHelper
    {
        /// <summary>
        /// 执行 Transact-SQL 增删改语句并返回受影响的行数。
        /// </summary>
        public int ExecuteNonQuery(string strConn, string cmdText)
        {
            if (string.IsNullOrEmpty(strConn) || string.IsNullOrEmpty(cmdText)) return 0;
            using (SqlConnection sqlConn = new SqlConnection(strConn))
            {
                sqlConn.Open();
                SqlCommand sqlCmd = sqlConn.CreateCommand();
                SqlTransaction sqlTran = sqlConn.BeginTransaction();
                try
                {
                    sqlCmd.CommandText = cmdText;
                    sqlCmd.Transaction = sqlTran;
                    int result = sqlCmd.ExecuteNonQuery();
                    sqlTran.Commit();//提交事务
                    return result;
                }
                catch (Exception exc)
                {
                    sqlTran.Rollback();
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
            using (SqlConnection sqlConn = new SqlConnection(strConn))
            {
                using (SqlCommand sqlCmd = sqlConn.CreateCommand())
                {
                    sqlCmd.CommandText = cmdText;
                    using (SqlDataAdapter sda = new SqlDataAdapter(sqlCmd))
                    {
                        DataTable dt = new DataTable();
                        try
                        {
                            sda.Fill(dt);
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
