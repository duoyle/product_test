using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows;
using System.Threading;
using System.Runtime.InteropServices;

namespace ProductTest
{
    public class CursorManager
    {
        private static Point crsrPosition;    //鼠标的位置

        /// <summary>
        /// 判断鼠标是否移动
        /// </summary>
        /// <returns></returns>
        public static bool CheckCrsrMoved()
        {
            Point point = GetMousePoint();//取得当前光标位置
            if (point == crsrPosition) return false;
            crsrPosition = point; return true;
        }

        #region 取得当前光标位置
        [StructLayout(LayoutKind.Sequential)]//类型定义
        private struct MPoint//点的类型定义（用于本地dll的类型）
        {
            public int X;
            public int Y;
            public MPoint(int x, int y)
            {
                this.X = x;
                this.Y = y;
            }
        }
        [DllImport("user32.dll", CharSet = CharSet.Auto)]//使用本地dll
        private static extern bool GetCursorPos(out MPoint mpt);//取得当前光标位置
        /// <summary>
        /// 获取当前屏幕鼠标位置
        /// </summary>
        /// <returns></returns>
        public static Point GetMousePoint()
        {
            MPoint mpt = new MPoint();
            GetCursorPos(out mpt);
            Point p = new Point(mpt.X, mpt.Y);
            return p;
        }
        #endregion

    }
}
