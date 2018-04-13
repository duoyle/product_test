using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProductTest
{
    public class MdlProductData
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

        //品号
        public string PrdctNumber
        {
            get;
            set;
        }

        //品名
        public string PrdctName
        {
            get;
            set;
        }

        //业体
        public string Industry
        {
            get;
            set;
        }

        //处理类型
        public string TreatType
        {
            get;
            set;
        }

        //白锈时长
        public string WeDuration
        {
            get;
            set;
        }

        //红锈时长
        public string ReDuration
        {
            get;
            set;
        }

        //备注
        public string Remark
        {
            get;
            set;
        }

    }
}
