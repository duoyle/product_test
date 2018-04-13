using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace ProductTest
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            CommonUtils.ProductIsAuthorized();
        }

        private void manageDatabase()
        {
            //if (File.Exists(System.Environment.CurrentDirectory + "\\ProductTest.mdb")) return;//存在则不创建库
            //// 不存在时创建数据库
            //CatalogClass ccDbMngr = new CatalogClass();
            //ccDbMngr.Create("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=C:\\Users\\XuXuemin\\Desktop\\Desktop\\"
            //    + "ProductTest.mdb;Jet OLEDB:Engine Type=5");
        }
    }
}
