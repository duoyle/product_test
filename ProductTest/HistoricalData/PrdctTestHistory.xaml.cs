using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.Data;
using System.Threading;
using System.Windows.Threading;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Reflection;
using Dongle.Utilities;
using DataAccess;

namespace ProductTest
{
    /// <summary>
    /// PrdctTestHistory.xaml 的交互逻辑
    /// </summary>
    public partial class PrdctTestHistory : Window
    {
        #region 全局变量
        //存储最终的修改
        private List<PrdctTestBindEntity> listPrdctTest = new List<PrdctTestBindEntity>();

        const int NumPerPage = 25;  //表示每页显示15条记录
        private int currentPage = 1;//当前第几页
        private int totalPage = 0;//总页数

        private string sqlCmd = string.Empty;

        private ReaderWriterLock rwLockLpt = new ReaderWriterLock();//全局对象"listPrdctTest"读写锁 xxm:13-11-21
        private const int CHECKINTERVAL = 10000;
        #endregion
        
        public PrdctTestHistory()
        {
            InitializeComponent();
            this.Icon = App.Current.MainWindow.Icon;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.firstLoad();
        }

        //第一次加载
        private void firstLoad()
        {
            this.dpStart.SelectedDate = DateTime.Today.AddDays(-7);
            this.dpEnd.SelectedDate = DateTime.Today;
            this.btnSearch_Click(null, null);//执行查询
        }

        #region 加载数据
        /// <summary>
        /// 排程刷新数据（线程调度）
        /// </summary>
        private void schedulingRefresh()
        {
            if (string.IsNullOrEmpty(this.sqlCmd)) return;//查询命令为空时返回
            //定义线程
            Thread thread = new Thread(new ThreadStart(loadData));
            //线程开启
            thread.Start();
        }

        /// <summary>
        /// 从库中读取对应条件的数据
        /// </summary>
        private void dbGetPrdctTest(string cmdText)
        {
            if (string.IsNullOrEmpty(cmdText)) return;
            try
            {
                rwLockLpt.AcquireWriterLock(CHECKINTERVAL);//申请写锁（超过设定时间退出等待）
                try
                {
                    this.listPrdctTest.Clear();//清空上次数据

                    DataTable dtPrdctTest = DatabaseUtils.GetDataTable(cmdText);
                    if (dtPrdctTest != null && dtPrdctTest.Rows.Count > 0)
                    {
                        int serNumber = 1;
                        foreach (DataRow item in dtPrdctTest.Rows)
                        {
                            PrdctTestBindEntity prdctTest = new PrdctTestBindEntity();
                            prdctTest.SerNumber = serNumber++;
                            prdctTest.Id = Convert.ToInt64(item["Id"]);
                            prdctTest.PrdctId = Convert.ToInt64(item["ProductId"]);
                            prdctTest.PrdctNumber = Convert.ToString(item["PrdctNumber"]);
                            prdctTest.TreatType = Convert.ToString(item["TreatType"]);//处理类型
                            prdctTest.WeDuration = Convert.ToString(item["WeDuration"]);//白锈时长
                            prdctTest.ReDuration = Convert.ToString(item["ReDuration"]);
                            prdctTest.StartTime = Convert.ToString(item["StartTime"]);
                            prdctTest.WeLossTime = Convert.ToString(item["WeLossTime"]);
                            prdctTest.ReLossTime = Convert.ToString(item["ReLossTime"]);
                            prdctTest.WeEndTime = Convert.ToString(item["WeEndTime"]);
                            prdctTest.ReEndTime = Convert.ToString(item["ReEndTime"]);
                            prdctTest.WeTestResult = Convert.ToString(item["WeTestResult"]);
                            prdctTest.ReTestResult = Convert.ToString(item["ReTestResult"]);
                            prdctTest.Remark = Convert.ToString(item["PrdctTestRemark"]);
                            this.listPrdctTest.Add(prdctTest);
                        }
                    }
                }
                finally
                {
                    rwLockLpt.ReleaseWriterLock();//释放写锁
                }
            }
            catch (Exception ex)
            {
                Logger.Error(MethodBase.GetCurrentMethod(), "加载产品数据异常：" + ex.Message);
            }
        }

        /// <summary>
        /// 加载数据
        /// </summary>
        /// <param name="objCmd"></param>
        private void loadData()
        {
            string cmdTxt = this.sqlCmd;//接收全局变量，以便后面使用
            if (string.IsNullOrEmpty(cmdTxt)) return;
            setCtrlEnableValue(false);//禁止控件
            //启动Timer更新进度条的状态
            ProgressManager pm = new ProgressManager(this.pbLoadData);
            pm.Start();//开始进度条
            //读取盐雾试验数据
            dbGetPrdctTest(cmdTxt);
            this.refreshItemCount();//刷新统计记录条数
            //绑定数据
            bindingPrdctTest(NumPerPage, this.currentPage);
            pm.Stop();//停止进度条
            setCtrlEnableValue(true);//恢复控件
        }

        /// <summary>
        /// 刷新行号列标题的统计条数
        /// </summary>
        private void refreshItemCount()
        {
            try
            {
                rwLockLpt.AcquireReaderLock(CHECKINTERVAL);//申请读锁
                try
                {
                    if (this.listPrdctTest == null) return;
                    this.Dispatcher.Invoke((Action)(() =>
                    {
                        this.dgPrdctTest.Columns[0].Header = "计：" + this.listPrdctTest.Count;
                    }));
                }
                finally
                {
                    rwLockLpt.ReleaseReaderLock();//释放读锁
                }
            }
            catch (Exception ex)
            {
                Logger.Error(MethodBase.GetCurrentMethod(), "刷新纪录条数：" + ex.Message);
            }
        }
        #endregion

        #region 数据列表分页
        /// <summary>
        /// number表示每个页面显示的记录数
        /// currentSize表示当前显示页数  
        /// </summary>
        /// <param name="number"></param>
        /// <param name="currentSize"></param>
        private void bindingPrdctTest(int number, int currentSize)
        {
            try
            {
                List<PrdctTestBindEntity> listPt = new List<PrdctTestBindEntity>();//定义当前页的数据源
                rwLockLpt.AcquireReaderLock(CHECKINTERVAL);//请求读锁
                try
                {
                    if (this.listPrdctTest == null) this.listPrdctTest = new List<PrdctTestBindEntity>();//若为null则初始化

                    int count = this.listPrdctTest.Count;          //获取记录总数   
                    int pageSize = 0;            //pageSize表示总页数   
                    if (count % number == 0)
                    {
                        pageSize = count / number;
                    }
                    else
                    {
                        pageSize = count / number + 1;
                    }

                    if (currentSize > pageSize) currentSize = pageSize; //xxm:如果当前页大于总页数则显示最后一页
                    if (currentSize < 1) currentSize = 1;               //xxm:如果当前页小于1则显示第一页

                    //刷选第currentSize页要显示的记录集
                    listPt = this.listPrdctTest.Take(number * currentSize).Skip(number * (currentSize - 1)).ToList();

                    this.totalPage = pageSize;          //更新全局标示的总页数
                    this.currentPage = currentSize;     //更新全局标示的第几页
                }
                finally
                {
                    rwLockLpt.ReleaseReaderLock();//释放读锁
                }
                this.Dispatcher.Invoke((Action)(() =>
                {
                    tbkTotal1.Text = this.totalPage.ToString();
                    tbkCurrentsize1.Text = this.currentPage.ToString();
                    this.dgPrdctTest.ItemsSource = listPt;        //重新绑定dataGrid1
                }));
            }
            catch (Exception ex)
            {
                Logger.Error(System.Reflection.MethodBase.GetCurrentMethod(), "显示选择设备数据异常：" + ex.Message);
            }
        }
        //跳到录入框指定页
        private void btnGo1_Click(object sender, RoutedEventArgs e)
        {
            string value = this.tbxPageNum1.Text;
            bool R = value.Trim() != string.Empty;
            R = System.Text.RegularExpressions.Regex.IsMatch(value.ToString(), @"^[1-9]\d{0,8}$");
            //R &= !Regex.IsMatch(value.ToString(), @"^[\uFF00-\uFFFF]*$");
            if (!R)
            {
                System.Windows.MessageBox.Show("请输入不超过9位数的正整数!", "系统提示", MessageBoxButton.OK, MessageBoxImage.Information);
                this.tbxPageNum1.Text = "";
                return;
            }
            if (string.IsNullOrEmpty(this.tbkTotal1.Text))
            {
                System.Windows.MessageBox.Show("请输入要跳转到的页数", "系统提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            int pageNum = int.Parse(tbxPageNum1.Text);
            int total = int.Parse(tbkTotal1.Text); //总页数   
            if (pageNum < 1 || pageNum > total)
            {
                System.Windows.MessageBox.Show("输入页码无效，必须为1到总页数之间的数字", "系统提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            bindingPrdctTest(NumPerPage, pageNum);     //调用分页方法   
        }
        //上一页
        private void btnUp1_Click(object sender, RoutedEventArgs e)
        {
            if (this.currentPage > 1)
            {
                bindingPrdctTest(NumPerPage, this.currentPage - 1);   //调用分页方法   
            }
        }
        //下一页
        private void btnNext1_Click(object sender, RoutedEventArgs e)
        {
            if (this.currentPage < this.totalPage)
            {
                bindingPrdctTest(NumPerPage, this.currentPage + 1);   //调用分页方法   
            }
        }
        #endregion

        #region 禁止恢复控件
        private void setCtrlEnableValue(bool value)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                this.btnSearch.IsEnabled = value;
                this.dgPrdctTest.ContextMenu.IsEnabled = value;
            }));
        }
        #endregion

        #region 其他事件处理
        //右键菜单功能
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is MenuItem)) return;
            MenuItem mi = (MenuItem)sender;
            switch (mi.Header.ToString())
            {
                case "删除":
                    if (this.dgPrdctTest.SelectedItem != null)
                    {
                        if (MessageBox.Show("确定要删除选中的试验数据吗？", "删除提示", MessageBoxButton.OKCancel, MessageBoxImage.Question)
                            == MessageBoxResult.Cancel)
                        {
                            return;
                        }
                        PrdctTestBindEntity prdctTest = this.dgPrdctTest.SelectedItem as PrdctTestBindEntity;
                        if (dbPrdctTestDelete(prdctTest.Id.ToString()))
                        {
                            schedulingRefresh();//刷新数据
                        }
                    }
                    else
                    {
                        MessageBox.Show("没有选中要删除的项", "系统提示", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                    break;
            }
        }

        //删除对应表名和ID的项
        private bool dbPrdctTestDelete(string idSel)
        {
            try
            {
                string sqlDel = string.Format("delete from ProductTest where Id={0}", idSel);
                return DatabaseUtils.ExecuteNonQuery(sqlDel) > 0;
            }
            catch (Exception ex)
            {
                Logger.Error(MethodBase.GetCurrentMethod(), "删除盐雾试验记录：" + ex.Message);
                return false;
            }
        }
        #endregion

        #region 查询数据
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (this.dpStart.SelectedDate > this.dpEnd.SelectedDate)
            {
                MessageBox.Show("开始日期不能大于结束日期", "系统提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            //Sqlite使用
            //string conditions = " where A.WeTestResult!='' and A.ReTestResult!=''";
            //Access使用
            string conditions = " where A.WeTestResult<>'' and A.ReTestResult<>''";
            //Sqlite使用
            //conditions += string.Format(" and datetime(A.StartTime)>=datetime('{0}') and datetime(A.StartTime)<datetime('{1}')",
            //    this.dpStart.SelectedDate.Value.Date.ToString("yyyy-MM-dd"), this.dpEnd.SelectedDate.Value.AddDays(1).Date.ToString("yyyy-MM-dd"));
            //Access使用
            conditions += string.Format(" and CDATE(A.StartTime)>=CDATE('{0}') and CDATE(A.StartTime)<CDATE('{1}')",
                this.dpStart.SelectedDate.Value.Date.ToString("yyyy-MM-dd"), this.dpEnd.SelectedDate.Value.AddDays(1).Date.ToString("yyyy-MM-dd"));
            if (!string.IsNullOrEmpty(this.tbPrdctNumber.Text.Trim()))
            {
                conditions += string.Format(" and B.PrdctNumber like '%{0}%'", this.tbPrdctNumber.Text.Trim());
            }
            if (!string.IsNullOrEmpty(this.tbTreatType.Text.Trim()))
            {
                conditions += string.Format(" and B.TreatType like '%{0}%'", this.tbTreatType.Text.Trim());
            }
            this.sqlCmd = "select A.Id,A.ProductId,B.PrdctNumber,B.PrdctName,B.Industry,B.TreatType,B.WeDuration,B.ReDuration," +
                    "B.Remark as PrdctRemark,A.StartTime,A.WeLossTime,A.ReLossTime,A.WeEndTime,A.ReEndTime,A.WeTestResult,A.ReTestResult," +
                    "A.Remark as PrdctTestRemark from ProductTest A left join ProductData B on A.ProductId=B.Id" + conditions;
            this.currentPage = 1;//重新查询时，当前页设为第一页
            schedulingRefresh();//刷新数据
        }
        #endregion

        #region 导出EXCEL
        //导出EXCEL按钮
        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                rwLockLpt.AcquireReaderLock(CHECKINTERVAL);//请求读锁
                try
                {
                    if (this.listPrdctTest == null || this.listPrdctTest.Count == 0)
                    {
                        System.Windows.MessageBox.Show("当前数据为空!", "系统提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }
                finally
                {
                    rwLockLpt.ReleaseReaderLock();//释放读锁
                }
                #region 提示保存文件窗
                Microsoft.Win32.SaveFileDialog sfdExport = new Microsoft.Win32.SaveFileDialog();
                sfdExport.Filter = "工作表文件(*.xls)|*.xls";
                sfdExport.Title = "保存文件";
                sfdExport.FileName = "盐雾试验历史数据" + DateTime.Today.ToString("yyyyMMdd");
                bool? dialogResult = sfdExport.ShowDialog();
                if (dialogResult == null || !dialogResult.Value) return;
                string fileName = sfdExport.FileName;
                #endregion
                Thread expThread = new Thread(new ParameterizedThreadStart(DataToExcel));
                //线程开启
                expThread.Start(fileName);
            }
            catch (Exception ex)
            {
                Logger.Error(MethodBase.GetCurrentMethod(), "导出数据异常：" + ex.Message);
            }
        }

        //调度线程导出数据到Excel
        private void DataToExcel(object objFn)
        {
            if (objFn == null) return;
            try
            {
                setCtrlEnableValue(false);//禁止控件
                //启动Timer更新进度条的状态
                ProgressManager pm = new ProgressManager(this.pbLoadData);
                pm.Start();//开始进度条
                string fileName = objFn.ToString();
                Dictionary<string, string> dicPropTitle = new Dictionary<string, string>();
                DataTable dt = null;
                rwLockLpt.AcquireReaderLock(CHECKINTERVAL);//请求读锁
                try
                {
                    dicPropTitle.Add("SerNumber", "计：" + this.listPrdctTest.Count);
                    dicPropTitle.Add("PrdctNumber", "品号");//品号
                    dicPropTitle.Add("TreatType", "处理类型");//处理类型
                    dicPropTitle.Add("WeDuration", "白锈时长");//白锈时长
                    dicPropTitle.Add("ReDuration", "红锈时长");
                    dicPropTitle.Add("StartTime", "开始时间");
                    dicPropTitle.Add("WeLossTime", "白锈LOSS");
                    dicPropTitle.Add("ReLossTime", "红锈LOSS");
                    dicPropTitle.Add("WeEndTime", "白锈结束时间");
                    dicPropTitle.Add("ReEndTime", "红锈结束时间");
                    dicPropTitle.Add("WeTestResult", "白锈结果");
                    dicPropTitle.Add("ReTestResult", "红锈结果");
                    dicPropTitle.Add("Remark", "备注");
                    dt = Dongle.Utilities.CommonUtils.ListToDataTable(this.listPrdctTest, "盐雾试验历史数据", dicPropTitle);
                }
                finally
                {
                    rwLockLpt.ReleaseReaderLock();//释放读锁
                }
                if (dt == null || dt.Rows.Count == 0)
                {
                    System.Windows.MessageBox.Show("转化后的数据为空!", "系统提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                DataSet ds = new DataSet("盐雾试验历史数据");
                ds.Tables.Add(dt);
                string mess = string.Empty;
                //bool result = DataImportExport.ExportExcel.DataToExcel(fileName, ds, ref mess);
                bool result = Dongle.Utilities.DataImportExport.DataToExcel(fileName, ds, ref mess);
                pm.Stop();//停止进度条
                setCtrlEnableValue(true);//恢复控件
                this.Dispatcher.BeginInvoke((Action)(() =>
                {
                    if (result)
                    {
                        System.Windows.MessageBox.Show("导出已完成!", "系统提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else if (!string.IsNullOrEmpty(mess))
                    {
                        System.Windows.MessageBox.Show(mess, "系统提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }));
            }
            catch (Exception ex)
            {
                Logger.Error(MethodBase.GetCurrentMethod(), "导出数据过程出现错误：" + ex.Message);
            }
        }
        #endregion

    }
}
