using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using DataAccess;
using Dongle.Utilities;
using System.Reflection;

namespace ProductTest
{
    public class ProductBindEntity : INotifyPropertyChanged
    {
        public ProductBindEntity() { }

        /// <summary>
        /// 属性变更
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// 属性变更执行方法
        /// </summary>
        /// <param name="name"></param>
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        //序号（行号）
        private int serNumber = 0;
        /// <summary>
        /// 序号（行号）
        /// </summary>
        public int SerNumber
        {
            get { return serNumber; }
            set
            {
                if (value != serNumber)
                {
                    serNumber = value;
                    OnPropertyChanged("SerNumber");
                }
            }
        }

        //标示是否可以修改编辑类型（由编辑类型决定如何提交到数据库）
        private bool updatableFlag = false;
        /// <summary>
        /// 标示是否可以修改编辑类型（由编辑类型决定如何提交到数据库）
        /// </summary>
        public bool UpdatableFlag
        {
            get { return updatableFlag; }
            set { updatableFlag = value; }
        }

        //编辑类型（新增、修改、删除、无改变）
        private EditState editState = EditState.Exist;
        /// <summary>
        /// 编辑类型（新增、修改、删除、无改变）
        /// </summary>
        public EditState EditState
        {
            get { return editState; }
            set { editState = value; }
        }

        //ID
        private long id = 0;
        /// <summary>
        /// ID
        /// </summary>
        public long Id
        {
            get { return id; }
            set
            {
                if (value != id)
                {
                    id = value;
                    OnPropertyChanged("Id");
                }
            }
        }

        //品号
        private string prdctNumber = string.Empty;
        /// <summary>
        /// 品号
        /// </summary>
        public string PrdctNumber
        {
            get { return prdctNumber; }
            set
            {
                if (value != prdctNumber)
                {
                    prdctNumber = value;
                    SetEditState(EditState.Modified);//设置为修改状态
                    OnPropertyChanged("PrdctNumber");
                }
            }
        }

        //品名
        private string prdctName = string.Empty;
        /// <summary>
        /// 品名
        /// </summary>
        public string PrdctName
        {
            get { return prdctName; }
            set
            {
                if (value != prdctName)
                {
                    prdctName = value;
                    SetEditState(EditState.Modified);//设置为修改状态
                    OnPropertyChanged("PrdctName");
                }
            }
        }

        //业体
        private string industry = string.Empty;
        /// <summary>
        /// 业体
        /// </summary>
        public string Industry
        {
            get { return industry; }
            set
            {
                if (value != industry)
                {
                    industry = value;
                    SetEditState(EditState.Modified);//设置为修改状态
                    OnPropertyChanged("Industry");
                }
            }
        }

        //处理类型
        private string treatType = string.Empty;
        /// <summary>
        /// 处理类型
        /// </summary>
        public string TreatType
        {
            get { return treatType; }
            set
            {
                if (value != treatType)
                {
                    treatType = value;
                    SetEditState(EditState.Modified);//设置为修改状态
                    OnPropertyChanged("TreatType");
                }
            }
        }

        //白锈时长
        private string weDuration = string.Empty;
        /// <summary>
        /// 白锈时长
        /// </summary>
        public string WeDuration
        {
            get { return weDuration; }
            set
            {
                if (value != weDuration)
                {
                    weDuration = value;
                    SetEditState(EditState.Modified);//设置为修改状态
                    OnPropertyChanged("WeDuration");
                }
            }
        }

        //红锈时长
        private string reDuration = string.Empty;
        /// <summary>
        /// 白锈时长
        /// </summary>
        public string ReDuration
        {
            get { return reDuration; }
            set
            {
                if (value != reDuration)
                {
                    reDuration = value;
                    SetEditState(EditState.Modified);//设置为修改状态
                    OnPropertyChanged("ReDuration");
                }
            }
        }

        //备注
        private string remark = string.Empty;
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark
        {
            get { return remark; }
            set
            {
                if (value != remark)
                {
                    remark = value;
                    OnPropertyChanged("Remark");
                }
            }
        }

        /// <summary>
        /// 操作数据库（修改产品属性）
        /// </summary>
        public void SaveProperty()
        {
            if (!this.updatableFlag) return;
            try
            {
                switch (EditState)
                {
                    case EditState.New:
                        //新增
                        string sqlNew = string.Format("INSERT INTO ProductData(PrdctNumber,PrdctName,Industry,TreatType,WeDuration,ReDuration,Remark)"
                            + " VALUES('{0}','{1}','{2}','{3}','{4}','{5}','{6}')", this.PrdctNumber, this.PrdctName, this.Industry, this.TreatType,
                            this.WeDuration, this.ReDuration, this.Remark);
                        DatabaseUtils.ExecuteNonQuery(sqlNew);
                        break;
                    case EditState.Deleted:
                        //删除
                        string sqlDel = string.Format("delete from ProductData where Id={0}", this.Id);
                        DatabaseUtils.ExecuteNonQuery(sqlDel);
                        break;
                    case EditState.Modified:
                        //修改
                        string sqlUpd = string.Format("UPDATE ProductData SET PrdctNumber='{0}',PrdctName='{1}',Industry='{2}',TreatType='{3}',"
                            + "WeDuration='{4}',ReDuration='{5}',Remark='{6}' WHERE Id={7}", this.PrdctNumber, this.PrdctName, this.Industry,
                            this.TreatType, this.WeDuration, this.ReDuration, this.Remark, this.Id);
                        DatabaseUtils.ExecuteNonQuery(sqlUpd);
                        break;
                }
            }
            catch (Exception ex)
            {
                //Logger.Error(MethodBase.GetCurrentMethod(), "产品基础资料管理错误：" + ex.Message);
                throw ex;
            }
        }

        /// <summary>
        /// 检查属性值是否合法
        /// </summary>
        /// <param name="tipMess"></param>
        /// <returns></returns>
        public bool IsValidValue(ref string tipMess)
        {
            if (string.IsNullOrEmpty(this.PrdctNumber))
            {
                tipMess = "品号不能为空";
                return false;
            }
            if (string.IsNullOrEmpty(this.ReDuration))
            {
                tipMess = "红锈时长不能为空";
                return false;
            }
            bool weDurIsEmpty = false;
            if (string.IsNullOrEmpty(this.WeDuration) || this.WeDuration == CommonUtils.EMPTYVALUE)
            {
                weDurIsEmpty = true;
                this.WeDuration = CommonUtils.EMPTYVALUE;
            }
            if (!weDurIsEmpty && !System.Text.RegularExpressions.Regex.IsMatch(this.WeDuration, @"^\d{1,7}(?:\.\d{1,2}$|$)"))
            {
                tipMess = "输入不合法，白锈时长为整数位不超过7位数，小数位不超过2位数的正整";
                return false;
            }
            if (!System.Text.RegularExpressions.Regex.IsMatch(this.ReDuration, @"^\d{1,7}(?:\.\d{1,2}$|$)"))
            {
                tipMess = "输入不合法，红锈时长为整数位不超过7位数，小数位不超过2位数的正整";
                return false;
            }
            try
            {
                if (!weDurIsEmpty && double.Parse(this.WeDuration) > double.Parse(this.ReDuration))
                {
                    tipMess = "红锈时长不能小于白锈时长";
                    return false;
                }
            }
            catch
            {
                tipMess = "你输入的内容不合法";
                return false;
            }
            return true;
        }

        /// <summary>
        /// 更新编辑状态
        /// </summary>
        /// <param name="parmState"></param>
        public void SetEditState(EditState parmState)
        {
            if (!this.UpdatableFlag) return;//当不允许更改状态时不做任何操作
            //当可更新时且编辑类型为Exist时更新传递过来的状态
            if (this.EditState == EditState.New)
            {
                //当编辑类型为New时不接受Modified状态的更新
                if (parmState != EditState.Modified) this.EditState = parmState;
            }
            else
            {
                this.EditState = parmState;
            }
        }
    }
}
