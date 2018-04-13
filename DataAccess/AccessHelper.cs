using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.OleDb;
using System.Data.Common;

namespace DataAccess
{
    /// <summary>
    /// 数据库操作实现类(for Sql Server)
    /// </summary>
    internal class AccessHelper : IDbHelper
    {
        /// <summary>
        /// 执行 Transact-SQL 增删改语句并返回受影响的行数。
        /// </summary>
        public int ExecuteNonQuery(string strConn, string cmdText)
        {
            if (string.IsNullOrEmpty(strConn) || string.IsNullOrEmpty(cmdText)) return 0;
            using (OleDbConnection oleDbConn = new OleDbConnection(strConn))
            {
                oleDbConn.Open();
                OleDbCommand oleDbCmd = oleDbConn.CreateCommand();
                OleDbTransaction oleDbTran = oleDbConn.BeginTransaction();
                try
                {
                    oleDbCmd.CommandText = cmdText;
                    oleDbCmd.Transaction = oleDbTran;
                    int result = oleDbCmd.ExecuteNonQuery();
                    oleDbTran.Commit();//提交事务
                    return result;
                }
                catch (Exception exc)
                {
                    oleDbTran.Rollback();
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
            using (OleDbConnection oleDbConn = new OleDbConnection(strConn))
            {
                using (OleDbCommand oleDbCmd = oleDbConn.CreateCommand())
                {
                    oleDbCmd.CommandText = cmdText;
                    using (OleDbDataAdapter odda = new OleDbDataAdapter(oleDbCmd))
                    {
                        DataTable dt = new DataTable();
                        try
                        {
                            odda.Fill(dt);
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
