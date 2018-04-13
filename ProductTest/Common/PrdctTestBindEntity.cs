using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace ProductTest
{
    public class PrdctTestBindEntity : INotifyPropertyChanged
    {
        public PrdctTestBindEntity()
        {
        }

        /// <summary>
        /// 属性变更
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
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

        private string imagePath;
        /// <summary>
        /// 图片路径
        /// </summary>
        public string ImagePath
        {
            get
            {
                return imagePath;
            }
            set
            {
                if (value != imagePath)
                {
                    imagePath = value;
                    OnPropertyChanged("ImagePath");
                }
            }
        }

        //ID
        private long id = 0;
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

        //产品ID
        private long prdctId = 0;
        public long PrdctId
        {
            get { return prdctId; }
            set
            {
                if (value != prdctId)
                {
                    prdctId = value;
                    OnPropertyChanged("PrdctId");
                }
            }
        }

        //品号
        private string prdctNumber = string.Empty;
        public string PrdctNumber
        {
            get { return prdctNumber; }
            set
            {
                if (value != prdctNumber)
                {
                    prdctNumber = value;
                    OnPropertyChanged("PrdctNumber");
                }
            }
        }

        //品名
        private string prdctName = string.Empty;
        public string PrdctName
        {
            get { return prdctName; }
            set
            {
                if (value != prdctName)
                {
                    prdctName = value;
                    OnPropertyChanged("PrdctName");
                }
            }
        }

        //处理类型
        private string treatType = string.Empty;
        public string TreatType
        {
            get { return treatType; }
            set
            {
                if (value != treatType)
                {
                    treatType = value;
                    OnPropertyChanged("TreatType");
                }
            }
        }

        //白锈时长
        private string weDuration = string.Empty;
        public string WeDuration
        {
            get { return weDuration; }
            set
            {
                if (value != weDuration)
                {
                    weDuration = value;
                    OnPropertyChanged("WeDuration");
                }
            }
        }

        //红锈时长
        private string reDuration = string.Empty;
        public string ReDuration
        {
            get { return reDuration; }
            set
            {
                if (value != reDuration)
                {
                    reDuration = value;
                    OnPropertyChanged("ReDuration");
                }
            }
        }

        //开始时间
        private string startTime = string.Empty;
        public string StartTime
        {
            get { return startTime; }
            set
            {
                if (value != startTime)
                {
                    startTime = value;
                    OnPropertyChanged("StartTime");
                }
            }
        }

        //白锈损失时间
        private string weLossTime = string.Empty;
        public string WeLossTime
        {
            get { return weLossTime; }
            set
            {
                if (value != weLossTime)
                {
                    weLossTime = value;
                    OnPropertyChanged("WeLossTime");
                }
            }
        }

        //红锈损失时间
        private string reLossTime = string.Empty;
        public string ReLossTime
        {
            get { return reLossTime; }
            set
            {
                if (value != reLossTime)
                {
                    reLossTime = value;
                    OnPropertyChanged("ReLossTime");
                }
            }
        }

        //白锈结束时间
        private string weEndTime = string.Empty;
        public string WeEndTime
        {
            get { return weEndTime; }
            set
            {
                if (value != weEndTime)
                {
                    weEndTime = value;
                    OnPropertyChanged("WeEndTime");
                }
            }
        }

        //红锈结束时间
        private string reEndTime = string.Empty;
        public string ReEndTime
        {
            get { return reEndTime; }
            set
            {
                if (value != reEndTime)
                {
                    reEndTime = value;
                    OnPropertyChanged("ReEndTime");
                }
            }
        }

        //白锈实验结果
        private string weTestResult = string.Empty;
        public string WeTestResult
        {
            get { return weTestResult; }
            set
            {
                if (value != weTestResult)
                {
                    weTestResult = value;
                    OnPropertyChanged("WeTestResult");
                }
            }
        }

        //白锈实验结果
        private string reTestResult = string.Empty;
        public string ReTestResult
        {
            get { return reTestResult; }
            set
            {
                if (value != reTestResult)
                {
                    reTestResult = value;
                    OnPropertyChanged("ReTestResult");
                }
            }
        }

        //备注
        private string remark = string.Empty;
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

        //格式化的开始时间
        private string fmtStartTime = string.Empty;
        public string FmtStartTime
        {
            get { return fmtStartTime; }
            set
            {
                if (value != fmtStartTime)
                {
                    fmtStartTime = value;
                    OnPropertyChanged("FmtStartTime");
                }
            }
        }

        //格式化的白锈结束时间
        private string fmtWeEndTime = string.Empty;
        public string FmtWeEndTime
        {
            get { return fmtWeEndTime; }
            set
            {
                if (value != fmtWeEndTime)
                {
                    fmtWeEndTime = value;
                    OnPropertyChanged("FmtWeEndTime");
                }
            }
        }

        //格式化的红锈结束时间
        private string fmtReEndTime = string.Empty;
        public string FmtReEndTime
        {
            get { return fmtReEndTime; }
            set
            {
                if (value != fmtReEndTime)
                {
                    fmtReEndTime = value;
                    OnPropertyChanged("FmtReEndTime");
                }
            }
        }

    }
}
