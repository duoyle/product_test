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
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Uri iconUri = new Uri("pack://application:,,,/sysicon.ico", UriKind.RelativeOrAbsolute);
            this.Icon = BitmapFrame.Create(iconUri);
        }

        #region 全局变量
        //存储最终的修改
        private List<PrdctTestBindEntity> listPrdctTest = new List<PrdctTestBindEntity>();

        const int NumPerPage = 25;  //表示每页显示15条记录
        private int currentPage = 1;//当前第几页
        private int totalPage = 0;//总页数

        private string sqlCmd = string.Empty;

        private ReaderWriterLock rwLockLpt = new ReaderWriterLock();//全局对象"listPrdctTest"读写锁 xxm:13-11-21

        private const string FLAGRED = @"/ProductTest;component/Resources/cycle_red.png";
        private const string FLAGGREEN = @"/ProductTest;component/Resources/cycle_green.png";

        private const int CHECKINTERVAL = 10000;
        #endregion

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.firstLoad();
        }

        //第一次加载
        private void firstLoad()
        {
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
                            prdctTest.Id = Convert.ToInt64(item["Id"]);//ID
                            prdctTest.PrdctId = Convert.ToInt64(item["ProductId"]);//产品ID
                            prdctTest.PrdctNumber = Convert.ToString(item["PrdctNumber"]);//品号
                            prdctTest.PrdctName = Convert.ToString(item["PrdctName"]);//品名
                            prdctTest.TreatType = Convert.ToString(item["TreatType"]);//处理类型
                            prdctTest.WeDuration = Convert.ToString(item["WeDuration"]);//白锈时长
                            prdctTest.ReDuration = Convert.ToString(item["ReDuration"]);//红锈时长
                            prdctTest.StartTime = Convert.ToString(item["StartTime"]);//开始时间
                            prdctTest.WeLossTime = Convert.ToString(item["WeLossTime"]);//白锈损失时长
                            prdctTest.ReLossTime = Convert.ToString(item["ReLossTime"]);//红锈损失时长
                            prdctTest.WeEndTime = Convert.ToString(item["WeEndTime"]);//白锈结束时间
                            prdctTest.ReEndTime = Convert.ToString(item["ReEndTime"]);//红锈结束时间
                            prdctTest.WeTestResult = Convert.ToString(item["WeTestResult"]);//白锈试验结果
                            prdctTest.ReTestResult = Convert.ToString(item["ReTestResult"]);//红锈试验结果
                            prdctTest.Remark = Convert.ToString(item["PrdctTestRemark"]);//备注
                            //开始时间格式取MM-dd HH:mm格式
                            prdctTest.FmtStartTime = (!string.IsNullOrEmpty(prdctTest.StartTime) && prdctTest.StartTime.Length == 19) ?
                                prdctTest.StartTime.Substring(5, 11) : prdctTest.StartTime;
                            //白锈结束时间格式取MM-dd HH:mm格式
                            prdctTest.FmtWeEndTime = (!string.IsNullOrEmpty(prdctTest.WeEndTime) && prdctTest.WeEndTime != CommonUtils.EMPTYVALUE
                                && prdctTest.WeEndTime.Length == 19) ? prdctTest.WeEndTime.Substring(5, 11) : prdctTest.WeEndTime;
                            //红锈结束时间格式取MM-dd HH:mm格式
                            prdctTest.FmtReEndTime = (!string.IsNullOrEmpty(prdctTest.ReEndTime) && prdctTest.ReEndTime != CommonUtils.EMPTYVALUE
                                && prdctTest.ReEndTime.Length == 19) ? prdctTest.ReEndTime.Substring(5, 11) : prdctTest.ReEndTime;
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
                Logger.Error(MethodBase.GetCurrentMethod(), "读取产品数据异常：" + ex.Message);
            }
        }
        
        /// <summary>
        /// 加载数据
        /// </summary>
        /// <param name="objCmd"></param>
        private void loadData()
        {
            string cmdTxt = this.sqlCmd;    //接收全局变量，以便后面使用
            if (string.IsNullOrEmpty(cmdTxt)) return;
            setCtrlEnableValue(false);      //禁止控件
            //启动Timer更新进度条的状态
            ProgressManager pm = new ProgressManager(this.pbLoadData);
            pm.Start();                     //开始进度条 让进度条产生进度更新
            //读取盐雾试验数据
            this.dbGetPrdctTest(cmdTxt);    //从库中读取数据
            this.refreshItemCount();        //刷新统计记录条数
            //绑定数据
            this.bindingPrdctTest(NumPerPage, this.currentPage);
            this.anewCheckTestResult();     //重新检查数据源是否有超时试验数据（有则提示并报警）
            pm.Stop();                      //停止进度条
            setCtrlEnableValue(true);       //恢复控件
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
                Logger.Error(MethodBase.GetCurrentMethod(), "刷新记录条数：" + ex.Message);
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
                this.btnNew.IsEnabled = value;
                this.btnPrdctData.IsEnabled = value;
                this.dgPrdctTest.ContextMenu.IsEnabled = value;
            }));
        }
        #endregion

        #region 实验数据增删改
        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            MdlProductTest mPrdctTest = new MdlProductTest();
            mPrdctTest.EditFlag = 1;//新增
            TestItemEdit tiEdit = new TestItemEdit(mPrdctTest);
            tiEdit.Refurbish += new TestItemEdit.DisplayUpdate(edit_Refurbish);//关联事件
            bool? result = tiEdit.ShowDialog();
            if (result != null && result.Value)
            {
                System.Windows.MessageBox.Show("添加成功！", "系统提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        //添加后触发的刷新事件
        private void edit_Refurbish(MdlProductTest item)
        {
            if (item == null) return;
            if (item.EditFlag == 1)
            {
                schedulingRefresh();//刷新数据
            }
            else if (item.EditFlag == 2)
            {
                schedulingRefresh();//刷新数据
            }
        }

        //管理产品基础数据
        private void btnPrdctData_Click(object sender, RoutedEventArgs e)
        {
            ProductData prdctData = new ProductData();
            prdctData.ShowDialog();
        }

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
                case "修改":
                    if (this.dgPrdctTest.SelectedItem != null)
                    {
                        PrdctTestBindEntity prdctTestSel = this.dgPrdctTest.SelectedItem as PrdctTestBindEntity;
                        MdlProductTest mptTemp = new MdlProductTest();
                        mptTemp.EditFlag = 2;
                        mptTemp.Id = prdctTestSel.Id;
                        mptTemp.PrdctDataIns = new MdlProductData();
                        mptTemp.PrdctDataIns.Id = prdctTestSel.PrdctId;
                        mptTemp.PrdctDataIns.PrdctNumber = prdctTestSel.PrdctNumber;
                        mptTemp.PrdctDataIns.TreatType = prdctTestSel.TreatType;
                        mptTemp.PrdctDataIns.WeDuration = prdctTestSel.WeDuration;
                        mptTemp.PrdctDataIns.ReDuration = prdctTestSel.ReDuration;
                        mptTemp.PrdctId = prdctTestSel.PrdctId;
                        mptTemp.StartTime = prdctTestSel.StartTime;
                        mptTemp.WeLossTime = prdctTestSel.WeLossTime;
                        mptTemp.ReLossTime = prdctTestSel.ReLossTime;
                        mptTemp.Remark = prdctTestSel.Remark;
                        TestItemEdit tiEdit = new TestItemEdit(mptTemp);
                        tiEdit.Refurbish += new TestItemEdit.DisplayUpdate(edit_Refurbish);//关联事件
                        bool? result = tiEdit.ShowDialog();
                        if (result != null && result.Value)
                        {
                            System.Windows.MessageBox.Show("修改成功！", "系统提示", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    else
                    {
                        MessageBox.Show("没有选中要修改的项", "系统提示", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                    break;
            }
        }

        //实验数据删除对应ID的项
        private bool dbPrdctTestDelete(string idSel)
        {
            if (string.IsNullOrEmpty(idSel)) return false;
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
            string conditions = " where (A.WeTestResult='' or A.ReTestResult='')";
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
            this.currentPage = 1;//改变搜索条件时，当前页设为第一页
            schedulingRefresh();//刷新数据
        }
        #endregion

        #region 轮询检查处理结果
        private System.Timers.Timer realTimer = null;
        
        /// <summary>
        /// 开启轮询检查
        /// </summary>
        private void startLoopCheck()
        {
            // 启动实时数据采样定时器
            if (realTimer == null)
            {
                realTimer = new System.Timers.Timer(CHECKINTERVAL);
                realTimer.Elapsed += new System.Timers.ElapsedEventHandler(checkTestResult);
                realTimer.AutoReset = true;
            }
            realTimer.Start();
        }

        private void checkTestResult(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (realTimer != null && realTimer.Interval != CHECKINTERVAL)
            {
                realTimer.Interval = CHECKINTERVAL;
            }
            compareToNow();//检查是否超过结束时间
        }

        /// <summary>
        /// 和当前时间对比检查是否超时
        /// </summary>
        private void compareToNow()
        {
            try
            {
                rwLockLpt.AcquireReaderLock(CHECKINTERVAL);//申请读锁（超过设定时间取不到退出）
                try
                {
                    if (this.listPrdctTest != null && this.listPrdctTest.Count > 0)
                    {
                        DateTime dtNow = DateTime.Now;
                        int countTimeout = 0;
                        foreach (PrdctTestBindEntity item in this.listPrdctTest)
                        {
                            if (string.IsNullOrEmpty(item.WeTestResult))    //白锈未处理
                            {
                                DateTime dtWeEnd = DateTime.Parse(item.WeEndTime);
                                if (dtNow.CompareTo(dtWeEnd) > 0)
                                {
                                    updImagePath(item, FLAGRED);//更新图片路径（警示用）
                                    countTimeout++;
                                }
                                else
                                {
                                    updImagePath(item, FLAGGREEN);
                                }
                            }
                            else if (string.IsNullOrEmpty(item.ReTestResult))   //红锈未处理
                            {
                                DateTime dtReEnd = DateTime.Parse(item.ReEndTime);
                                if (dtNow.CompareTo(dtReEnd) > 0)
                                {
                                    updImagePath(item, FLAGRED);
                                    countTimeout++;
                                }
                                else
                                {
                                    updImagePath(item, FLAGGREEN);
                                }
                            }
                        }
                        if (countTimeout > 0)
                        {
                            this.Dispatcher.BeginInvoke((Action)(() =>
                            {
                                BeepUp.Beep(500, 700);
                            }));
                        }
                    }
                }
                finally
                {
                    rwLockLpt.ReleaseReaderLock();
                }
            }
            catch (Exception ex)
            {
                Logger.Error(MethodBase.GetCurrentMethod(), "轮询检查：" + ex.Message);
            }
        }
        /// <summary>
        /// 更新图片路径
        /// </summary>
        /// <param name="item"></param>
        /// <param name="imgPath"></param>
        private void updImagePath(PrdctTestBindEntity item, string imgPath)
        {
            if (item == null) return;
            if (item.ImagePath != imgPath)
            {
                //申请升级到写操作
                LockCookie lc = rwLockLpt.UpgradeToWriterLock(CHECKINTERVAL);
                try
                {
                    item.ImagePath = imgPath;
                }
                finally
                {
                    rwLockLpt.DowngradeFromWriterLock(ref lc);//回到调用UpgradeToWriterLock前的状态
                }
            }
        }

        /// <summary>
        /// 关闭轮询检查
        /// </summary>
        private void stopLoopCheck()
        {
            // 启动实时数据采样定时器
            if (realTimer != null)
            {
                realTimer.Stop();
                realTimer = null;
            }
        }
        
        /// <summary>
        /// 重新检查试验结果（超时检查）
        /// </summary>
        private void anewCheckTestResult()
        {
            this.stopLoopCheck();//停止检查
            //WpfApplication.DoEvents();//处理事件
            compareToNow();//检查是否超时
            this.startLoopCheck();//开启轮询
        }
        #endregion

        #region 处理实验结果
        //右键菜单白锈处理结果
        private void MenuItemWeRes_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is MenuItem)) return;
            MenuItem mi = (MenuItem)sender;
            switch (mi.Header.ToString())
            {
                case "合格":
                    if (this.dgPrdctTest.SelectedItem != null)
                    {
                        PrdctTestBindEntity prdctTest = this.dgPrdctTest.SelectedItem as PrdctTestBindEntity;
                        if (prdctTest.WeTestResult == CommonUtils.EMPTYVALUE)
                        {
                            MessageBox.Show("选中项不做白锈试验", "系统提示", MessageBoxButton.OK, MessageBoxImage.Information);
                            return;
                        }
                        if (MessageBox.Show("确定要将该试验记录的白锈试验结果设为合格吗？", "修改提示", MessageBoxButton.OKCancel, MessageBoxImage.Question)
                            == MessageBoxResult.Cancel)
                        {
                            return;
                        }
                        if (dbPrdctTestUpd_Prop("WeTestResult", "合格", prdctTest.Id.ToString()))
                        {
                            schedulingRefresh();//刷新数据
                        }
                    }
                    else
                    {
                        MessageBox.Show("没有选中要处理的项", "系统提示", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                    break;
                case "不合格":
                    if (this.dgPrdctTest.SelectedItem != null)
                    {
                        PrdctTestBindEntity prdctTest = this.dgPrdctTest.SelectedItem as PrdctTestBindEntity;
                        if (prdctTest.WeTestResult == CommonUtils.EMPTYVALUE)
                        {
                            MessageBox.Show("选中项不做白锈试验", "系统提示", MessageBoxButton.OK, MessageBoxImage.Information);
                            return;
                        }
                        if (MessageBox.Show("确定要将该试验记录的白锈试验结果设为不合格吗？", "修改提示", MessageBoxButton.OKCancel, MessageBoxImage.Question)
                            == MessageBoxResult.Cancel)
                        {
                            return;
                        }
                        if (dbPrdctTestUpd_Prop("WeTestResult", "不合格", prdctTest.Id.ToString()))
                        {
                            //如果白锈不合格（失败）则红锈也不合格
                            if (dbPrdctTestUpd_Prop("ReTestResult", "不合格", prdctTest.Id.ToString()))
                            {
                                schedulingRefresh();//刷新数据
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("没有选中要处理的项", "系统提示", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                    break;
            }
        }

        //右键菜单红锈处理结果
        private void MenuItemReRes_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is MenuItem)) return;
            MenuItem mi = (MenuItem)sender;
            switch (mi.Header.ToString())
            {
                case "合格":
                    if (this.dgPrdctTest.SelectedItem != null)
                    {
                        if (MessageBox.Show("确定要将该试验记录的红锈试验结果设为合格吗？", "修改提示", MessageBoxButton.OKCancel, MessageBoxImage.Question)
                            == MessageBoxResult.Cancel)
                        {
                            return;
                        }
                        PrdctTestBindEntity prdctTest = this.dgPrdctTest.SelectedItem as PrdctTestBindEntity;
                        if (dbPrdctTestUpd_Prop("ReTestResult", "合格", prdctTest.Id.ToString()))
                        {
                            schedulingRefresh();//刷新数据
                        }
                    }
                    else
                    {
                        MessageBox.Show("没有选中要处理的项", "系统提示", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                    break;
                case "不合格":
                    if (this.dgPrdctTest.SelectedItem != null)
                    {
                        if (MessageBox.Show("确定要将该试验记录的红锈试验结果设为不合格吗？", "修改提示", MessageBoxButton.OKCancel, MessageBoxImage.Question)
                            == MessageBoxResult.Cancel)
                        {
                            return;
                        }
                        PrdctTestBindEntity prdctTest = this.dgPrdctTest.SelectedItem as PrdctTestBindEntity;
                        if (dbPrdctTestUpd_Prop("ReTestResult", "不合格", prdctTest.Id.ToString()))
                        {
                            schedulingRefresh();//刷新数据
                        }
                    }
                    else
                    {
                        MessageBox.Show("没有选中要处理的项", "系统提示", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                    break;
            }
        }

        /// <summary>
        /// 修改对应ID和字段的试验数据
        /// </summary>
        /// <param name="idSel"></param>
        /// <returns></returns>
        private bool dbPrdctTestUpd_Prop(string propName, string propValue, string idSel)
        {
            if (string.IsNullOrEmpty(propName) || string.IsNullOrEmpty(propValue) || string.IsNullOrEmpty(idSel)) return false;
            try
            {
                string sqlDel = string.Format("update ProductTest set {0}='{1}' where Id={2}", propName, propValue, idSel);
                return DatabaseUtils.ExecuteNonQuery(sqlDel) > 0;
            }
            catch (Exception ex)
            {
                Logger.Error(MethodBase.GetCurrentMethod(), "删除盐雾试验记录：" + ex.Message);
                return false;
            }
        }
        #endregion

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (this.realTimer != null)
            {
                this.realTimer.Stop();
                this.realTimer = null;
            }
        }

        #region 试验历史数据
        private void btnPrdctTestHis_Click(object sender, RoutedEventArgs e)
        {
            PrdctTestHistory pth = new PrdctTestHistory();
            pth.Show();
        }
        #endregion

        #region 全屏显示
        private void btnFullScreen_Click(object sender, RoutedEventArgs e)
        {
            FullScreenWindow fullScrWin = new FullScreenWindow(this.listPrdctTest);
            fullScrWin.Show();
            this.Hide();
            fullScrWin.Closed += new EventHandler(fullScrWin_Closed);
        }

        private void fullScrWin_Closed(object sender, EventArgs e)
        {
            this.Show();//全屏显示退出后显示当前窗体
        }
        #endregion

    }

}
