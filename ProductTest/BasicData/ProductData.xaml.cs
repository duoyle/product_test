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
using System.ComponentModel;
using Dongle.Utilities;
using System.Reflection;
using System.Collections.ObjectModel;
using System.Data;
using System.Threading;
using DataAccess;

namespace ProductTest
{
    /// <summary>
    /// ProductData.xaml 的交互逻辑
    /// </summary>
    public partial class ProductData : Window
    {
        #region 全局变量
        //存储最终的修改
        private List<ProductBindEntity> listProduct = new List<ProductBindEntity>();
        //绑定的数据源
        private ObservableCollection<ProductBindEntity> ocProduct = new ObservableCollection<ProductBindEntity>();
        #endregion

        public ProductData()
        {
            InitializeComponent();
            this.dgProductData.Sorting += new DataGridSortingEventHandler(dgProductData_Sorting);
            this.Closing += new CancelEventHandler(ProductData_Closing);
            this.Icon = App.Current.MainWindow.Icon;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.firstLoad();
        }

        //第一次加载
        private void firstLoad()
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(loadData));
        }

        #region 关闭时检查更改保存
        //关闭前事件
        private void ProductData_Closing(object sender, CancelEventArgs e)
        {
            if (this.productsIsUpdated())
            {
                if (MessageBox.Show("已经修改，如果退出将不保存这些修改，是否退出？", "系统提示",
                                    MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.No)
                {
                    e.Cancel = true;
                }
            }
        }
        //检查是否有修改
        private bool productsIsUpdated()
        {
            foreach (ProductBindEntity item in this.listProduct)
            {
                if (item.EditState != EditState.Exist)
                {
                    return true;
                }
            }
            return false;
        }
        #endregion

        #region 加载数据
        //读取产品数据
        private void dbGetProduct()
        {
            try
            {
                string sql = "select Id,PrdctNumber,PrdctName,Industry,TreatType,WeDuration,ReDuration,Remark from ProductData";
                DataTable dtProduct = DatabaseUtils.GetDataTable(sql);
                if (dtProduct != null && dtProduct.Rows.Count > 0)
                {
                    listProduct.Clear();

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
                Logger.Error(MethodBase.GetCurrentMethod(), "保存产品数据异常：" + ex.Message);
            }
        }

        //绑定下拉列表数据源
        public void bindingItemsSource()
        {
            try
            {
                #region 绑定选择处理类型
                //string devTypeSql = "select Id,CODE,NAME from s_dict_object where DICTCLASSID='100008000000000037'";
                //DataTable dtDevType = DataBase.This.ExecuteSQL(devTypeSql);
                //Dictionary<string, string> dicDevType = new Dictionary<string, string>();
                //dicDevType.Add("100008000000000037", "==全部==");
                //if (dtDevType != null && dtDevType.Rows.Count > 0)
                //{
                //    listDevType.Clear();
                //    foreach (DataRow drType in dtDevType.Rows)
                //    {
                //        long typeId = Convert.ToInt64(drType["Id"]);
                //        string typeName = Convert.ToString(drType["NAME"]);
                //        dicDevType.Add(typeId.ToString(), typeName);
                //        listDevType.Add(new IntPortDeviceType(typeId, typeName));//添加DataGrid设备类型下拉列表数据源
                //    }
                //}
                #endregion
            }
            catch (Exception ex)
            {
                Logger.Error(MethodBase.GetCurrentMethod(), "管理交换机ComboBox数据绑定异常：" + ex.Message);
            }
        }

        //加载数据
        private void loadData(object obj)
        {
            this.setCtrlEnableValue(false);//禁止控件活跃
            showProgressBar(10);//显示进度条（每次10个进度格）
            //读取所有产品数据
            dbGetProduct();
            //根据检索框的筛选条件筛选数据作为绑定到界面的数据源
            this.bindingDataFilter();
            hideProgressBar();//隐藏工具条
            this.setCtrlEnableValue(true);//恢复控件
        }

        /// <summary>
        /// 筛选List为绑定的数据源并绑定到界面
        /// </summary>
        private void bindingDataFilter()
        {
            string prdctNumber = string.Empty;//品号
            string treatType = string.Empty;//处理类型
            this.Dispatcher.Invoke((Action)(() =>
            {
                prdctNumber = this.tbPrdctNumber.Text.Trim();
                treatType = this.tbTreatType.Text.Trim();
            }));
            ObservableCollection<ProductBindEntity> ocIntPortTemp = new ObservableCollection<ProductBindEntity>();//临时存储筛选后的对象
            foreach (ProductBindEntity item in this.listProduct)
            {
                //listProduct.Where(x => x.PrdctNumber.Contains(prdctNumber)).ToList();
                if (item.EditState != EditState.Deleted &&
                    item.PrdctNumber.Contains(prdctNumber) && item.TreatType.Contains(treatType))//根据选择的ip筛选数据
                {
                    ocIntPortTemp.Add(item);
                }
                refreshProgressBar();//更新进度条
            }
            this.Dispatcher.Invoke((Action)(() =>
            {
                this.ocProduct = ocIntPortTemp;
                GC.Collect();//回收ocProduct所占的数据
                this.dgProductData.ItemsSource = this.ocProduct;
                this.updOcProductBindSn();//更新行号
                if (this.ocProduct.Count > 0)
                {
                    this.dgProductData.UpdateLayout();
                    this.dgProductData.ScrollIntoView(this.ocProduct[0]);
                }
            }));
            refreshProgressBar();//更新进度条
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

        #region 缓存修改
        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            ProductBindEntity item = new ProductBindEntity();
            item.SerNumber = ocProduct.Count > 0 ? ocProduct.Max(x => x.SerNumber) + 1 : 1;
            item.UpdatableFlag = true;//允许更改编辑类型
            item.SetEditState(EditState.New);//设置编辑状态为新增
            this.listProduct.Add(item);//更新缓存数据
            this.ocProduct.Add(item);//更新绑定数据源
            //选中新增的行，并滚动到该行
            this.dgProductData.SelectedItem = item;
            this.dgProductData.ScrollIntoView(item);
        }

        //删除选择的项
        private void deleteProductSel()
        {
            if (this.dgProductData.SelectedItem == null)
            {
                MessageBox.Show("请选择要删除的项", "系统提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            ProductBindEntity item = this.dgProductData.SelectedItem as ProductBindEntity;
            if (item.EditState == EditState.New)//新增的直接移除
            {
                this.listProduct.Remove(item);
            }
            else
            {
                //更改编辑类型为Deleted
                item.SetEditState(EditState.Deleted);
            }
            this.ocProduct.Remove(item);//从数据源移除
            this.updOcProductBindSn();//更新行号
        }

        //更新行号
        private void updOcProductBindSn()
        {
            int serNum = 1;
            foreach (ProductBindEntity item in this.ocProduct)
            {
                item.SerNumber = serNum++;
            }
        }
        #endregion

        #region 保存修改
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (this.listProduct == null) return;
            //检查每一项输入是否合法
            foreach (ProductBindEntity item in this.listProduct)
            {
                string mess = string.Empty;
                if (!item.IsValidValue(ref mess))
                {
                    //this.cbSelDevice.SelectedIndex = 0;
                    MessageBox.Show(mess);
                    return;
                }
            }
            ThreadPool.QueueUserWorkItem(new WaitCallback(dbSaveIntPort));
        }

        //保存产品数据
        private void dbSaveIntPort(object obj)
        {
            if (this.listProduct == null) return;
            this.setCtrlEnableValue(false);//禁用操作控件
            showProgressBar(this.listProduct.Count);//显示进度条
            try
            {
                //保存到数据库
                foreach (ProductBindEntity item in this.listProduct)
                {
                    item.SaveProperty();
                    item.SetEditState(EditState.Exist);//保存后更改编辑状态
                    refreshProgressBar();//更新进度条
                }

                asyncShowMessage("保存成功！");
            }
            catch (Exception ex)
            {
                asyncShowMessage("数据保存过程中遇到问题");
                Logger.Error(MethodBase.GetCurrentMethod(), "保存产品数据异常：" + ex.Message);
            }
            this.hideProgressBar();//关闭进度条
            this.setCtrlEnableValue(true);//恢复操作控件
        }

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
            //从缓存记录中筛选数据更新数据源
            ThreadPool.QueueUserWorkItem(new WaitCallback(updDgItemsSource), null);
        }

        /// <summary>
        /// 更新绑定的数据源
        /// </summary>
        /// <param name="obj"></param>
        private void updDgItemsSource(object obj)
        {
            this.setCtrlEnableValue(false);//禁止控件活跃
            showProgressBar(this.listProduct.Count + 1);//显示进度条（每次10个进度格）
            //根据检索框的筛选条件筛选数据作为绑定到界面的数据源
            this.bindingDataFilter();
            hideProgressBar();//隐藏工具条
            this.setCtrlEnableValue(true);//恢复控件
        }
        #endregion

        #region 禁止恢复控件
        private void setCtrlEnableValue(bool value)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                this.btnSelect.IsEnabled = value;
                this.btnNew.IsEnabled = value;
                this.btnSave.IsEnabled = value;
            }));
        }
        #endregion

        //右键菜单点击事件
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem)
            {
                switch (((MenuItem)sender).Header.ToString())
                {
                    case "删除":
                        deleteProductSel();
                        break;
                }
            }
        }

    }

}
