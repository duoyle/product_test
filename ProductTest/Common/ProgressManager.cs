using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace ProductTest
{
    /// <summary>
    /// 进度条管理
    /// </summary>
    public class ProgressManager
    {
        System.Timers.Timer timer = null;//使用线程池调度的Timer
        //管理的进度条实体
        System.Windows.Controls.ProgressBar progBar = null;
        /// <summary>
        /// 刷新的时间间隔（毫秒）
        /// </summary>
        public double UpdInterval = 20.0;
        /// <summary>
        /// 进度条的最大值
        /// </summary>
        public double MaxValue = 20;

        /// <summary>
        /// 用进度条对象实例化进度条管理类
        /// </summary>
        /// <param name="pb">要展示的进度条实例</param>
        public ProgressManager(System.Windows.Controls.ProgressBar pb)
        {
            this.progBar = pb;
        }

        /// <summary>
        /// 开始刷新进度条
        /// </summary>
        public void Start()
        {
            if (progBar == null) return;
            if (timer == null)
            {
                timer = new System.Timers.Timer();
                timer.Interval = UpdInterval;
                timer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Elapsed);
                showProgressBar(MaxValue, this.progBar);
                timer.Start();
            }
        }

        /// <summary>
        /// 关闭进度条
        /// </summary>
        public void Stop()
        {
            if (timer != null)
            {
                timer.Stop();
                timer = null;
            }
            hideProgressBar(this.progBar);
        }

        private void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            refreshProgressBar(this.progBar);
        }

        #region 进度条管理
        //time每次增加一个进度条的进度栏
        private static void refreshProgressBar(System.Windows.Controls.ProgressBar pb)
        {
            if (pb == null) return;
            pb.Dispatcher.Invoke((Action)(() =>
            {
                if (pb.Value == pb.Maximum)
                {
                    pb.Value = 0;
                }
                pb.Value += 1;
            }));
        }
        //显示进度条
        private static void showProgressBar(double maximum, System.Windows.Controls.ProgressBar pb)
        {
            if (pb == null) return;
            pb.Dispatcher.Invoke((Action)(() =>
            {
                pb.Minimum = 0;
                pb.Maximum = maximum;//一轮进度最大值
                pb.Value = 0;
                pb.Visibility = Visibility.Visible;
            }));
        }
        //隐藏进度条
        private static void hideProgressBar(System.Windows.Controls.ProgressBar pb)
        {
            if (pb == null) return;
            pb.Dispatcher.Invoke((Action)(() =>
            {
                pb.Visibility = Visibility.Hidden;
            }));
        }
        #endregion

    }

}
