using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;

namespace DataAccess
{
    interface IDbHelper
    {
        /// <summary>
        /// 执行 SQL 增删改语句并返回受影响的行数。
        /// </summary>
        int ExecuteNonQuery(string strConn, string cmdText);

        /// <summary>
        /// 执行 SQL 查询，返回DataTable
        /// </summary>
        DataTable GetDataTable(string strConn, string cmdText);

    }
}
