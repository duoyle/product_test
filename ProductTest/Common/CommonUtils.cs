using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Reflection;
using System.Collections;

namespace ProductTest
{
    public enum EditState
    {
        Exist = 1,
        New = 2,
        Deleted = 3,
        Modified = 4
    }

    public static class CommonUtils
    {
        public static void ProductIsAuthorized()
        {
            DateTime dtOverdue = DateTime.Parse("2018-12-31 00:00:00");
            if (DateTime.Now.CompareTo(dtOverdue) > 0)
            {
                System.Windows.MessageBox.Show("对不起，该产品已经超过有效期！", "系统提示", System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information);
                App.Current.Shutdown();
            }
        }

        const double GB = 1024 * 1024 * 1024;//定义GB的计算常量
        const double MB = 1024 * 1024;//定义MB的计算常量
        const double KB = 1024;//定义KB的计算常量
        public static string ByteConversionKbmbgb(double byteSize, int decPlace = 2)
        {
            string strFormat = "f2";
            if (decPlace > 0)
            {
                strFormat = "f" + decPlace;
            }
            if (byteSize / GB >= 1)//如果当前Byte的值大于等于1GB
                return (byteSize / GB).ToString(strFormat) + " GB";//将其转换成GB
            else if (byteSize / MB >= 1)//如果当前Byte的值大于等于1MB
                return (byteSize / MB).ToString(strFormat) + " MB";//将其转换成MB
            else if (byteSize / KB >= 1)//如果当前Byte的值大于等于1KB
                return (byteSize / KB).ToString(strFormat) + " KB";//将其转换成KGB
            else
                return byteSize.ToString(strFormat) + " Byte";//显示Byte值
        }

        /// <summary>
        /// 空值或无此项时的取代值，用"无"取代
        /// </summary>
        public const string EMPTYVALUE = "无";

        /// <summary>
        /// 序列化对象为XML，泛型必须为类，且具有无参构造方法，类中需要解析的应定义为属性
        /// 属性必须为值类型或List，若为List则泛型必须为类，和上述类的要求相同，以此类推所有的类都是如此
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static string SerializeXml<T>(T obj) where T : class, new()
        {
            StringBuilder sbXml = new StringBuilder();      //声明保存XML字符串的可变字符串对象
            if (obj == null) return sbXml.ToString();       //传递为空对象时退出
            try
            {
                Type type = obj.GetType();                  //取得实例的类型
                //取得对象的所有属性
                PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                if (properties == null || properties.Length <= 0)
                {
                    return sbXml.ToString();
                }
                sbXml.AppendFormat("<{0}>", type.Name);     //开始符
                //遍历属性，把属性值写入到XML节点
                foreach (PropertyInfo prop in properties)
                {
                    //从实例中读取对应该属性的值
                    object value = prop.GetValue(obj, null);
                    if (value == null) continue;
                    if (prop.PropertyType.IsValueType || prop.PropertyType == typeof(string))
                    {
                        //直接读取值类型和string类型的内容
                        //对XML节点的内容做特殊字符处理
                        string innerText = FilterXmlSpecial(value.ToString());
                        //生成对应属性的节点和节点的内容
                        sbXml.AppendFormat("<{0}>{1}</{0}>", prop.Name, innerText);
                    }
                    else if (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
                    {
                        //对List<T>的处理
                        sbXml.AppendFormat("<{0}>", prop.Name);                     //用属性名生成List的开始父节点
                        IList iList = value as IList;                               //转为IList对象以遍历每项
                        foreach (var ilItem in iList)
                        {
                            Type typeItem = ilItem.GetType();                       //取得每项的类型
                            if (typeItem.IsClass && typeItem != typeof(string))     //类型必须是类，且不能为string
                            {
                                sbXml.Append(SerializeXml(ilItem));  //递归调用解析每项的值
                            }
                        }
                        sbXml.AppendFormat("</{0}>", prop.Name);                    //用属性名生成List的结束父节点
                    }
                }
                sbXml.AppendFormat("</{0}>", type.Name);    //结束符
            }
            catch (Exception ex)
            {
                Dongle.Utilities.Logger.Error(System.Reflection.MethodBase.GetCurrentMethod(), "序列化XML错误:" + ex.Message);
            }
            return sbXml.ToString();
        }

        /// <summary>
        /// 反序列化XML到对象，泛型必须为类，且具有无参构造方法，类中需要解析的应定义为属性
        /// 属性必须为值类型或List，若为List则泛型必须为类，和上述类的要求相同，以此类推所有的类都是如此
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="xmlNode"></param>
        public static void DeserializeXml<T>(T obj, XmlNode xmlNode) where T : class, new()
        {
            //泛型在未做限制前不能赋值null，因为其可能为值类型，但可以==null进行判断，最好做限制
            //class和new()不是同一约束，结构体也可以有构造方法
            if (obj == null || xmlNode == null || xmlNode.ChildNodes == null) return;

            try
            {
                XmlNodeList xnlProperties = xmlNode.ChildNodes;             //属性节点
                Type type = obj.GetType();                                  //取得对象的类型
                //遍历所有属性节点
                foreach (XmlNode xnProp in xnlProperties)
                {
                    PropertyInfo pi = type.GetProperty(xnProp.Name);        //是否存在此属性检测
                    if (pi != null)
                    {
                        //处理List<T>类型的属性
                        if (pi.PropertyType.IsGenericType && pi.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
                        {
                            XmlNodeList xnList = xnProp.ChildNodes;
                            if (xnList == null) continue;                                       //List下的子节点不存在继续下个属性
                            IList iList = Activator.CreateInstance(pi.PropertyType) as IList;   //创建List对象
                            Type[] ts = pi.PropertyType.GetGenericArguments();                  //取得泛型类型的实际类型数组
                            Type genType = ts[0];                                               //因为List中的泛型只有一种，取第一个
                            foreach (XmlNode xnItem in xnList)
                            {
                                var genInst = Activator.CreateInstance(genType);                //反射创建泛型类型实例
                                DeserializeXml(genInst, xnItem);                               //递归调用解析泛型内的属性
                                iList.Add(genInst);
                            }
                            type.GetProperty(xnProp.Name).SetValue(obj, iList, null);           //为属性赋值
                        }
                        if (pi.PropertyType.IsValueType || pi.PropertyType == typeof(string))   //属性的类型
                        {
                            //xml节点中的值转为属性实际的类型
                            object value = Convert.ChangeType(xnProp.InnerText, pi.PropertyType);
                            //为属性设置值
                            type.GetProperty(xnProp.Name).SetValue(obj, value, null);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Dongle.Utilities.Logger.Error(System.Reflection.MethodBase.GetCurrentMethod(), "反序列XML到对象错误：" + ex.Message);
            }
        }

        /// <summary>
        /// xml特殊字符替换
        /// </summary>
        /// <param name="innerText">节点的内容</param>
        /// <returns></returns>
        public static string FilterXmlSpecial(string innerText)
        {
            if (string.IsNullOrEmpty(innerText)) return innerText;
            innerText = innerText.Replace("&", "&amp;");
            innerText = innerText.Replace("<", "&lt;");
            innerText = innerText.Replace(">", "&gt;");
            innerText = innerText.Replace("'", "&apos;");
            innerText = innerText.Replace("\"", "&quot;");
            innerText = innerText.Replace("\0", "0");
            return innerText;
        }

    }
}
