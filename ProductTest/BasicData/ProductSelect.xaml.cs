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
using System.Threading;
using System.Data;
using Dongle.Utilities;
using System.Reflection;
using DataAccess;

namespace ProductTest
{
    /// <summary>
    /// TestResult.xaml 的交互逻辑
    /// </summary>
    public partial class ProductSelect : Window
    {
        #region 全局变量和属性
        //存储最终的修改
        private List<ProductBindEntity> listProduct = new List<ProductBindEntity>();

        private ProductBindEntity prdctSelect = null;//选择的产品
        /// <summary>
        /// 选择的产品
        /// </summary>
        public ProductBindEntity PrdctSelect
        {
            get { return prdctSelect; }
        }

        /// <summary>
        /// 当前使用的sql查询命令
        /// </summary>
        private string sqlCmd = string.Empty;
        #endregion

        public ProductSelect()
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
            this.sqlCmd = "select Id,PrdctNumber,PrdctName,Industry,TreatType,WeDuration,ReDuration,Remark from ProductData";
            schedulingRefresh();
        }

        #region 加载数据
        /// <summary>
        /// 排程刷新数据（线程调度）
        /// </summary>
        private void schedulingRefresh()
        {
            if (string.IsNullOrEmpty(this.sqlCmd)) return;//查询命令为空时返回
            //定义线程
            Thread thread = new Thread(new ParameterizedThreadStart(loadData));
            //线程开启
            thread.Start(this.sqlCmd);
        }

        //读取产品数据
        private void dbGetProduct(string cmdSelect)
        {
            if (string.IsNullOrEmpty(cmdSelect)) return;
            try
            {
                listProduct.Clear();//清空缓存数据
                DataTable dtProduct = DatabaseUtils.GetDataTable(cmdSelect);
                if (dtProduct != null && dtProduct.Rows.Count > 0)
                {
                    foreach (DataRow item in dtProduct.Rows)
                    {
                        ProductBindEntity prodEnt = new ProductBindEntity();
                        prodEnt.Id = Convert.ToInt64(item["Id"]);
                        prodEnt.PrdctNumber = Convert.ToString(item["PrdctNumber"]);
                        prodEnt.PrdctName = Convert.ToString(item["PrdctName"]);
                        prodEnt.Industry = Convert.ToString(item["Industry"]);
                        prodEnt.TreatType = Convert.ToString(item["TreatType"]);//处理类型
                        prodEnt.WeDuration = Convert.ToString(item["WeDuration"]);//白锈时长
                        prodEnt.ReDuration = Convert.ToString(item["ReDuration"]);
                        prodEnt.Remark = Convert.ToString(item["Remark"]);
                        prodEnt.UpdatableFlag = true;//允许更改编辑类型
                        listProduct.Add(prodEnt);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(MethodBase.GetCurrentMethod(), "选择产品筛选异常：" + ex.Message);
            }
        }

        //加载数据
        private void loadData(object objCmd)
        {
            if (objCmd == null) return;
            string cmdTxt = objCmd.ToString();
            //启动Timer更新进度条的状态
            ProgressManager pm = new ProgressManager(this.pbLoadData);
            pm.Start();//开始进度条
            //读取所有产品数据
            dbGetProduct(cmdTxt);
            bindingProduct();//绑定数据
            pm.Stop();//停止进度条
        }

        /// <summary>
        /// 绑定数据源
        /// </summary>
        private void bindingProduct()
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                this.dgProductData.ItemsSource = this.listProduct;        //重新绑定dataGrid1
            }));
        }
        #endregion

        #region DataGrid点击列标题排序
        //排序前事件
        private void dgProductData_Sorting(object sender, DataGridSortingEventArgs e)
        {
            e.Handled = true;
        }
        #endregion

        #region 进度条管理
        //time每次增加一个进度条的进度栏
        private void refreshProgressBar()
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                if (this.pbLoadData.Value == this.pbLoadData.Maximum)
                {
                    this.pbLoadData.Value = 0;
                }
                this.pbLoadData.Value += 1;
                //WpfApplication.DoEvents();
            }));
        }
        //显示进度条
        private void showProgressBar(int maximum)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                this.pbLoadData.Minimum = 0;
                this.pbLoadData.Maximum = maximum;//一轮进度最大值
                this.pbLoadData.Value = 0;
                this.pbLoadData.Visibility = Visibility.Visible;
            }));
        }
        //隐藏进度条
        private void hideProgressBar()
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                this.pbLoadData.Visibility = Visibility.Hidden;
            }));
        }
        #endregion

        #region 异步提示信息
        //异步提示信息
        private void asyncShowMessage(string mess)
        {
            this.Dispatcher.BeginInvoke((Action)(() =>
            {
                MessageBox.Show(mess);
            }));
        }
        #endregion

        #region 筛选数据
        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            string conditions = " where 1=1";
            if (!string.IsNullOrEmpty(this.tbPrdctNumber.Text.Trim()))
            {
                conditions += string.Format(" and PrdctNumber like '%{0}%'", this.tbPrdctNumber.Text.Trim());
            }
            if (!string.IsNullOrEmpty(this.tbTreatType.Text.Trim()))
            {
                conditions += string.Format(" and TreatType like '%{0}%'", this.tbTreatType.Text.Trim());
            }
            this.sqlCmd = "select Id,PrdctNumber,PrdctName,Industry,TreatType,WeDuration,ReDuration,Remark from ProductData" + conditions;
            schedulingRefresh();//刷新数据
        }
        #endregion

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            if (this.dgProductData.SelectedItem == null)
            {
                MessageBox.Show("没有选中的项", "系统提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            this.prdctSelect = this.dgProductData.SelectedItem as ProductBindEntity;
            this.DialogResult = true;
        }

    }
}
