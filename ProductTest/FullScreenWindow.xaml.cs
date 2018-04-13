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
using System.Windows.Shapes;
using Dongle.Utilities;
using System.Reflection;
using System.Windows.Threading;

namespace ProductTest
{
    /// <summary>
    /// FullScreenWindow.xaml 的交互逻辑
    /// </summary>
    public partial class FullScreenWindow : Window
    {
        #region 全局变量
        private List<PrdctTestBindEntity> listPrdctTest = null;//主窗体（父窗体）实例

        private const int countPerPage = 12;  //表示每页显示记录条数
        private int currentPage = 1;//当前第几页
        private int totalPage = 0;//总页数

        private const string margin = "\x20\x20";
        private const string RealTimeFormat = "yyyy-MM-dd  HH:mm" + margin;//实时时间格式字符串

        private System.Timers.Timer realTimer = null;//数据刷新计时器
        private System.Timers.Timer scrollTimer = null;//翻屏计时器
        //光标管理
        private DispatcherTimer cursorTimer = new DispatcherTimer(DispatcherPriority.Background);//光标管理计时器
        private int intCrsrChkCount = 0;//检查次数
        #endregion

        public FullScreenWindow(List<PrdctTestBindEntity> _listPrdctTest)
        {
            InitializeComponent();
            this.listPrdctTest = _listPrdctTest;    //设置数据源
            this.initializeWindow();    //初始化窗口
        }

        private void cursorTimer_Tick(object sender, EventArgs e)
        {
            if (this.Cursor == Cursors.Arrow)
            {
                this.intCrsrChkCount++;
                if (this.intCrsrChkCount > 3) this.Cursor = Cursors.None;
            }
        }

        private void FullScreenWindow_MouseMove(object sender, MouseEventArgs e)
        {
            this.intCrsrChkCount = 0;
            this.Cursor = Cursors.Arrow;
        }

        private void FullScreenWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (this.realTimer != null)
            {
                this.realTimer.Stop();
                this.realTimer = null;
            }
            if (this.scrollTimer != null)
            {
                this.scrollTimer.Stop();
                this.scrollTimer = null;
            }
            if (this.cursorTimer != null)
            {
                this.cursorTimer.Stop();
                this.cursorTimer = null;
            }
        }

        #region 初始化窗体数据
        /// <summary>
        /// 窗口初始化
        /// </summary>
        private void initializeWindow()
        {
            this.Closing += new System.ComponentModel.CancelEventHandler(FullScreenWindow_Closing);//窗体关闭事件
            this.MouseMove += new MouseEventHandler(FullScreenWindow_MouseMove);//鼠标移动事件
            //检查光标
            this.cursorTimer.Interval = new TimeSpan(0, 0, 1);
            this.cursorTimer.Tick += new EventHandler(cursorTimer_Tick);
            this.cursorTimer.Start();
            //全屏模式
            this.WindowState = System.Windows.WindowState.Normal;
            this.WindowStyle = System.Windows.WindowStyle.None;
            this.ResizeMode = System.Windows.ResizeMode.NoResize;
            //this.Topmost = true;  //在最前
            this.Left = 0.0;
            this.Top = 0.0;
            this.Width = System.Windows.SystemParameters.PrimaryScreenWidth;
            this.Height = System.Windows.SystemParameters.PrimaryScreenHeight;
            //显示实时时间
            this.realtimeScheduling();
            //绑定数据
            this.initBindingData();
        }

        /// <summary>
        /// 初始化绑定数据
        /// </summary>
        private void initBindingData()
        {
            //数据绑定初始化
            try
            {
                this.refreshItemCount();//显示记录条数
                //设置数据源翻屏数
                if (this.listPrdctTest != null && countPerPage > 0)
                {
                    int countTotal = this.listPrdctTest.Count;          //获取记录总数   
                    if (countTotal % countPerPage == 0)
                    {
                        this.totalPage = countTotal / countPerPage;
                    }
                    else
                    {
                        this.totalPage = countTotal / countPerPage + 1;
                    }
                }
                this.refreshBindingData(countPerPage, 1);//绑定数据 显示第一页
                //如果页数大于2，启动动态翻屏效果
                if (this.totalPage > 1)
                {
                    scrollTimer = new System.Timers.Timer(15 * 1000);//设置翻屏计时器间隔
                    scrollTimer.Elapsed += new System.Timers.ElapsedEventHandler(scrollTimer_Elapsed);
                    scrollTimer.Start();//开启翻屏显示
                }
            }
            catch (Exception ex)
            {
                Logger.Error(System.Reflection.MethodBase.GetCurrentMethod(), "全屏显示初始化错误：" + ex.Message);
            }
        }

        /// <summary>
        /// 翻屏显示
        /// </summary>
        private void scrollTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            this.currentPage += 1;//页数加1
            if (this.currentPage > this.totalPage) this.currentPage = 1;//如果超过总页数则显示第一页
            this.refreshBindingData(countPerPage, this.currentPage);//刷新页面数据
        }

        /// <summary>
        /// 实时时间调度
        /// </summary>
        private void realtimeScheduling()
        {
            this.tbDtNow.Text = DateTime.Now.ToString(RealTimeFormat);//显示当前时间
            realTimer = new System.Timers.Timer(1000);//设置数据刷新计时器间隔
            realTimer.Elapsed += new System.Timers.ElapsedEventHandler(realTimer_Elapsed);
            realTimer.Start();
        }

        /// <summary>
        /// 刷新时间事件
        /// </summary>
        private void realTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            this.Dispatcher.BeginInvoke((Action)(() =>
            {
                if (this.tbDtNow.Text != DateTime.Now.ToString(RealTimeFormat))
                {
                    this.tbDtNow.Text = DateTime.Now.ToString(RealTimeFormat);
                    //WpfApplication.DoEvents();
                }
            }));
        }

        /// <summary>
        /// 刷新行号列标题的统计条数
        /// </summary>
        private void refreshItemCount()
        {
            try
            {
                if (this.listPrdctTest == null) return;
                this.Dispatcher.Invoke((Action)(() =>
                {
                    this.dgPrdctTest.Columns[2].Header = "共:" + this.listPrdctTest.Count;
                }));
            }
            catch (Exception ex)
            {
                Logger.Error(MethodBase.GetCurrentMethod(), "刷新记录录条数：" + ex.Message);
            }
        }
        #endregion

        /// <summary>
        /// 绑定数据
        /// </summary>
        /// <param name="_countCurrPage">当前页的行数</param>
        /// <param name="_currentPage">当前第几页</param>
        private void refreshBindingData(int _countCurrPage, int _currentPage)
        {
            try
            {
                List<PrdctTestBindEntity> listPt = new List<PrdctTestBindEntity>();//定义当前页的数据源                

                if (_currentPage > totalPage) _currentPage = totalPage; //如果当前页大于总页数则显示最后一页
                if (_currentPage < 1) _currentPage = 1;               //如果当前页小于1则显示第一页

                //刷选第currentSize页要显示的记录集
                listPt = this.listPrdctTest.Take(_countCurrPage * _currentPage).Skip(_countCurrPage * (_currentPage - 1)).ToList();

                this.currentPage = _currentPage;
                this.Dispatcher.Invoke((Action)(() =>
                {
                    this.tbScrInfo.Text = margin + this.currentPage.ToString() + "/" + this.totalPage.ToString();//更新全局标示的第几页
                    this.dgPrdctTest.ItemsSource = listPt;//重新绑定dataGrid1
                }));
            }
            catch (Exception ex)
            {
                Logger.Error(System.Reflection.MethodBase.GetCurrentMethod(), "全屏显示设备数据异常：" + ex.Message);
            }
        }

        /// <summary>
        /// 重写父类方法（退出键结束全屏）
        /// </summary>
        /// <param name="e"></param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Key == Key.Escape)
            {
                this.Close();//关闭当前窗口
            }
        }
    }
}
