using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProductTest
{
    public class MdlProductTest
    {
        /// <summary>
        /// 编辑标识，1 新增，2 修改
        /// </summary>
        public int EditFlag
        {
            get;
            set;
        }

        /// <summary>
        /// ID
        /// </summary>
        public long Id
        {
            get;
            set;
        }

        /// <summary>
        /// 产品ID对应的产品实例
        /// </summary>
        public MdlProductData PrdctDataIns
        {
            get;
            set;
        }

        /// <summary>
        /// 产品ID
        /// </summary>
        public long PrdctId
        {
            get;
            set;
        }

        /// <summary>
        /// 开始时间
        /// </summary>
        public string StartTime
        {
            get;
            set;
        }

        /// <summary>
        /// 白锈损失时间
        /// </summary>
        public string WeLossTime
        {
            get;
            set;
        }

        /// <summary>
        /// 红锈损失时间
        /// </summary>
        public string ReLossTime
        {
            get;
            set;
        }

        /// <summary>
        /// 白锈结束时间
        /// </summary>
        public string WeEndTime
        {
            get;
            set;
        }

        /// <summary>
        /// 红锈结束时间
        /// </summary>
        public string ReEndTime
        {
            get;
            set;
        }

        /// <summary>
        /// 白锈试验结果
        /// </summary>
        public string WeTestResult
        {
            get;
            set;
        }

        /// <summary>
        /// 红锈实验结果
        /// </summary>
        public string ReTestResult
        {
            get;
            set;
        }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark
        {
            get;
            set;
        }
    }
}
