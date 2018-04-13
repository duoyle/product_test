using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SQLite;
using System.Configuration;
using Dongle.Utilities;
using System.Reflection;

namespace DataAccess
{
    /// <summary>
    /// 数据库操作实现类（for Sqlite）
    /// </summary>
    internal class SqliteHelper : IDbHelper
    {
        /// <summary>
        /// 执行 SQL 增删改语句并返回受影响的行数。
        /// </summary>
        public int ExecuteNonQuery(string strConn, string cmdText)
        {
            if (string.IsNullOrEmpty(strConn) || string.IsNullOrEmpty(cmdText)) return 0;
            //使用using以便释放连接对象
            using (SQLiteConnection sqliteConn = new SQLiteConnection(strConn))
            {
                sqliteConn.Open();
                SQLiteCommand sqliteCmd = sqliteConn.CreateCommand();//创建Sqlite命令实例
                SQLiteTransaction sqliteTran = sqliteConn.BeginTransaction();//创建连接的事务实例
                try
                {
                    sqliteCmd.CommandText = cmdText;//执行的sql语句
                    sqliteCmd.Transaction = sqliteTran;//事务机制
                    int result = sqliteCmd.ExecuteNonQuery();
                    sqliteTran.Commit();//提交事务
                    return result;
                }
                catch (Exception exc)
                {
                    sqliteTran.Rollback();//回滚事务
                    throw exc;
                }
            }
        }

        /// <summary>
        /// 执行 SQL 查询，返回DataTable
        /// </summary>
        public DataTable GetDataTable(string strConn, string cmdText)
        {
            if (string.IsNullOrEmpty(strConn) || string.IsNullOrEmpty(cmdText)) return null;
            //使用using以便释放连接对象
            using (SQLiteConnection sqliteConn = new SQLiteConnection(strConn))
            {
                SQLiteCommand sqliteCmd = sqliteConn.CreateCommand();//创建Sqlite命令实例
                sqliteCmd.CommandText = cmdText;
                using (SQLiteDataAdapter sda = new SQLiteDataAdapter(sqliteCmd))
                {
                    //SQLiteDataAdapter实例会自动打开连接，所以无需调用Open方法
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
