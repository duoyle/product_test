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
using Xceed.Wpf.Toolkit;
using Dongle.Utilities;
using System.Reflection;
using DataAccess;

namespace ProductTest
{
    /// <summary>
    /// TestItemEdit.xaml 的交互逻辑
    /// </summary>
    public partial class TestItemEdit : Window
    {
        #region 全局对象
        private MdlProductTest mpt;
        public MdlProductTest Mpt
        {
            get { return mpt; }
        }

        /// <summary>
        /// 定义一个委托
        /// </summary>
        public delegate void DisplayUpdate(MdlProductTest item);

        /// <summary>
        /// 定义一个事件，执行刷新
        /// </summary>
        public virtual event DisplayUpdate Refurbish;
        #endregion

        public TestItemEdit(MdlProductTest _mpt)
        {
            InitializeComponent();
            this.mpt = _mpt;
            this.Icon = App.Current.MainWindow.Icon;
        }

        #region 自定义方法
        /// <summary>
        /// 展示数据
        /// </summary>
        private void displayData()
        {
            if (this.mpt != null)
            {
                if (this.mpt.PrdctDataIns != null)
                {
                    this.tbPrdctNum.Text = this.mpt.PrdctDataIns.PrdctNumber;           //显示品号
                    if (this.mpt.PrdctDataIns.WeDuration == CommonUtils.EMPTYVALUE)
                    {
                        this.tbWeLossTime.IsEnabled = false;
                        this.tbWeLossTime.Text = "不做白锈试验";
                    }
                    else
                    {
                        this.tbWeLossTime.Text = this.mpt.WeLossTime;
                        this.tbWeLossTime.IsEnabled = true;
                    }
                }
                this.dtpStartTime.Value = Convert.ToDateTime(this.mpt.StartTime);       //显示开始时间
                this.tbReLossTime.Text = this.mpt.ReLossTime;                           //显示红锈损失时长
                this.tbRemark.Text = this.mpt.Remark;                                   //显示备注
            }
        }
        #endregion

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.mpt.EditFlag == 1)
            {
                this.btnSelPrdct.Focus();
                this.mpt.StartTime = DateTime.Now.ToString();
            }
            else if (this.mpt.EditFlag == 2)
            {
            }
            displayData();//显示数据
        }

        /// <summary>
        /// 验证输入的合法性
        /// </summary>
        /// <returns></returns>
        private bool validateInput()
        {
            if (this.tbPrdctNum.Text.Trim() == "" || this.mpt.PrdctDataIns == null)
            {
                System.Windows.MessageBox.Show("请选择产品！", "系统提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }
            if (this.dtpStartTime.Value == null)
            {
                System.Windows.MessageBox.Show("请选择开始时间！", "系统提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }
            bool weTest = this.mpt.PrdctDataIns.WeDuration != CommonUtils.EMPTYVALUE;
            if (weTest && this.tbWeLossTime.Text.Trim() == "")
            {
                System.Windows.MessageBox.Show("请输入白锈损失时间！", "系统提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }
            if (this.tbReLossTime.Text.Trim() == "")
            {
                System.Windows.MessageBox.Show("请输入红锈损失时间！", "系统提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }
            if (weTest && !System.Text.RegularExpressions.Regex.IsMatch(this.tbWeLossTime.Text, @"^\d{1,7}(?:\.\d{1,2}$|$)"))
            {
                System.Windows.MessageBox.Show("白锈损失时间为整数位不超过7位数，小数位不超过2位数的正整！", "系统提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }
            if (!System.Text.RegularExpressions.Regex.IsMatch(this.tbReLossTime.Text, @"^\d{1,7}(?:\.\d{1,2}$|$)"))
            {
                System.Windows.MessageBox.Show("红锈损失时间为整数位不超过7位数，小数位不超过2位数的正整！", "系统提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }
            try
            {
                if (weTest)
                {
                    if ((double.Parse(this.tbWeLossTime.Text.Trim()) + double.Parse(mpt.PrdctDataIns.WeDuration)) >
                        (double.Parse(this.tbReLossTime.Text.Trim()) + double.Parse(mpt.PrdctDataIns.ReDuration)))
                    {
                        System.Windows.MessageBox.Show("计算的结束时间不合法：白锈结束时间不能晚于红锈结束时间！", "系统提示",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        return false;
                    }
                }
            }
            catch
            {
                System.Windows.MessageBox.Show("输入不合法！", "系统提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }
            return true;
        }

        /// <summary>
        /// 设置属性值
        /// </summary>
        private void setProperties()
        {
            this.mpt.PrdctId = this.mpt.PrdctDataIns.Id;
            this.mpt.StartTime = this.dtpStartTime.Value.Value.ToString("yyyy-MM-dd HH:mm:ss");
            //白锈不需要做实验的判断（以产品信息中白锈时长为"无"作为标识）
            if (this.mpt.PrdctDataIns.WeDuration == CommonUtils.EMPTYVALUE)
            {
                this.mpt.WeLossTime = CommonUtils.EMPTYVALUE;
                this.mpt.WeEndTime = CommonUtils.EMPTYVALUE;
                this.mpt.WeTestResult = CommonUtils.EMPTYVALUE;
            }
            else
            {
                this.mpt.WeLossTime = this.tbWeLossTime.Text.Trim();
                this.mpt.WeEndTime = DateTime.Parse(mpt.StartTime).AddHours(double.Parse(mpt.WeLossTime) + double.Parse(mpt.PrdctDataIns.WeDuration)).ToString("yyyy-MM-dd HH:mm:ss");
                this.mpt.WeTestResult = string.Empty;
            }
            this.mpt.ReLossTime = this.tbReLossTime.Text.Trim();
            this.mpt.ReEndTime = DateTime.Parse(mpt.StartTime).AddHours(double.Parse(mpt.ReLossTime) + double.Parse(mpt.PrdctDataIns.ReDuration)).ToString("yyyy-MM-dd HH:mm:ss");
            this.mpt.ReTestResult = string.Empty;
            this.mpt.Remark = this.tbRemark.Text.Trim();
        }

        private void btnSelPrdct_Click(object sender, RoutedEventArgs e)
        {
            ProductSelect prdctSel = new ProductSelect();
            bool? result = prdctSel.ShowDialog();
            if (result != null && result.Value)
            {
                if (prdctSel.PrdctSelect != null)
                {
                    //设置选择产品的属性值
                    this.mpt.PrdctDataIns = new MdlProductData();
                    this.mpt.PrdctDataIns.Id = prdctSel.PrdctSelect.Id;
                    this.mpt.PrdctDataIns.PrdctNumber = prdctSel.PrdctSelect.PrdctNumber;
                    this.mpt.PrdctDataIns.TreatType = prdctSel.PrdctSelect.TreatType;
                    this.mpt.PrdctDataIns.WeDuration = prdctSel.PrdctSelect.WeDuration;
                    this.mpt.PrdctDataIns.ReDuration = prdctSel.PrdctSelect.ReDuration;
                    this.mpt.WeLossTime = string.Empty; //选中产品后重置白锈损失时间
                    this.mpt.ReLossTime = string.Empty; //选中产品后重置红锈损失时间
                    displayData();//显示数据
                }
            }
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            if (!validateInput()) return;//判断输入是否合法
            try
            {
                this.setProperties();//设置属性值
                if (this.mpt.EditFlag == 1)//添加
                {
                    string sql = string.Format("insert into ProductTest(ProductId,StartTime,WeLossTime,ReLossTime,WeEndTime,ReEndTime," +
                        "WeTestResult,ReTestResult,Remark) values({0},'{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}')", mpt.PrdctId,
                        mpt.StartTime, mpt.WeLossTime, mpt.ReLossTime, mpt.WeEndTime, mpt.ReEndTime, mpt.WeTestResult, mpt.ReTestResult, mpt.Remark);
                    if (DatabaseUtils.ExecuteNonQuery(sql) > 0)
                    {
                        this.Refurbish(this.mpt);
                        this.DialogResult = true;
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("添加失败！", "系统提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else if (this.mpt.EditFlag == 2)//修改
                {
                    string sql = string.Format("update ProductTest set ProductId={0},StartTime='{1}',WeLossTime='{2}',ReLossTime='{3}'," +
                        "WeEndTime='{4}',ReEndTime='{5}',WeTestResult='{6}',ReTestResult='{7}',Remark='{8}' where Id={9}", mpt.PrdctId, mpt.StartTime,
                        mpt.WeLossTime, mpt.ReLossTime, mpt.WeEndTime, mpt.ReEndTime, mpt.WeTestResult, mpt.ReTestResult, mpt.Remark, mpt.Id);
                    if (DatabaseUtils.ExecuteNonQuery(sql) > 0)
                    {
                        this.Refurbish(this.mpt);
                        this.DialogResult = true;
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("修改失败！", "系统提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(MethodBase.GetCurrentMethod(), "修改产品试验数据异常：" + ex.Message);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }
}
