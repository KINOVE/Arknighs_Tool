﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;//线程
using System.Configuration;//配置文件
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;//读写Json
using System.IO;

namespace Arknights_Tool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Read();
            Set_StartUpLocation();
            showtimer = new DispatcherTimer();//实例化
            showtimer.Tick += new EventHandler(ShowCurLz);//对应的每次触发的事件（计算用）
            showtimer.Start();//开启时间，这里先开启，会直接刷新一次，然后再调整为六分钟刷新一次
            showtimer.Interval = new TimeSpan(0, 0, 3, 0);//控制时间六分钟跳动一次
        }


        //声明计时器
        private DispatcherTimer showtimer;

        public void ShowCurLz(object sender, EventArgs e)   //显示当前理智
        {
            Cal_Lz();
        }

        public void Cal_Lz()    //计算剩余理智
        {

            lizhi_now.Text = Convert.ToString(Info.info.lz_start + Math.Floor((double)((DateTime.Now - Info.info.time_start).TotalMinutes / 6)));
            lizhi_full.Text = Convert.ToString(Info.info.lz_full);
            TimeSpan ts1 = Info.info.time_full.Subtract(DateTime.Now);
            if(ts1 < TimeSpan.Zero)//如果已经回满了
            {
                lizhi_now.Text = lizhi_full.Text; //限定为最大值
                caution_img.Visibility = Visibility.Visible; //打开警告图标显示
                caution.Visibility = Visibility.Visible;    //打开警告文字显示
                last_time.Visibility = Visibility.Collapsed;    //关闭正常文字显示
                caution_text.Text = "已于";
            }
            else //如果没有回满
            {
                caution_img.Visibility = Visibility.Collapsed; //关闭警告图标显示
                caution.Visibility = Visibility.Collapsed;    //关闭警告文字显示
                last_time.Visibility = Visibility.Visible;    //打开正常文字显示
                caution_text.Text = "将于";
                if (ts1.Days == 0)
                {
                    last_time.Text = ts1.Hours.ToString() + " 小时 "
                                + ts1.Minutes.ToString() + " 分钟 ";
                }
                else
                {
                    last_time.Text = " " + ts1.Days.ToString() + " 天 "
                                + ts1.Hours.ToString() + " 小时 "
                                + ts1.Minutes.ToString() + " 分钟 ";
                }
            }
            
            if (Info.info.time_full.Day == DateTime.Now.Day)
            {
                time_test.Text = (Info.info.time_full).ToString(" HH:mm ");
            }
            else
            {
                time_test.Text = (Info.info.time_full).ToString(" 明天 HH:mm ");
            }


            PB_1.Value = Convert.ToDouble(lizhi_now.Text) / Info.info.lz_full * 100;
        }


        public void GetNow(string value1, string value2)
        {
            try
            {
                lizhi_now.Text = value1;//将数字转为double ----> Convert.ToString(value1)
                lizhi_full.Text = value2;

                Info.info.lz_start = Convert.ToDouble(value1);
                Info.info.lz_full = Convert.ToDouble(value2);
                Info.info.time_start = DateTime.Now;
                double minutes = (Info.info.lz_full - Info.info.lz_start) * 6; //需要回复多少分钟
                Info.info.time_full = DateTime.Now.AddMinutes(minutes);

                Write();
                Cal_Lz();
            }
            catch (Exception ex)
            {
                MessageBox.Show("发生异常 " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }//更改进度条上的数值文字（委托方法）


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //childWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            //childWindow.WindowStartupLocation = WindowStartupLocation.Manual;
            ChildWindow childWindow = new ChildWindow();
            childWindow.getLizhi = GetNow;
            childWindow.Left = this.Left;
            childWindow.Top = 115 + this.Top;
            childWindow.ShowDialog();

        }//【更改体力】按钮点击


        public class Info
        {
            public DateTime time_start = Convert.ToDateTime("2022-02-15T14:52:13.7122306+08:00"); //记录时间
            public DateTime time_full = Convert.ToDateTime("2022-02-16T02:52:13.7122337+08:00");  //回满时间
            public double lz_start = 1.0; //初始理智
            public double lz_full = 133.0;  //理智上限

            //创建静态对象
            public static Info info = new Info();

        }//负责存储【记录时间】【回满时间】【初始理智】【理智上限】

        public void Read()
        {
            if (!File.Exists("info.json"))
            {
                File.Create("info.json").Close();
                Write();
            }
            else
            {
                StreamReader file = File.OpenText("info.json");
                JsonTextReader reader = new JsonTextReader(file);
                JObject jsonObject = (JObject)JToken.ReadFrom(reader);
                Info.info.time_start = (DateTime)jsonObject["time_start"];
                Info.info.time_full = (DateTime)jsonObject["time_full"];
                Info.info.lz_start = (double)jsonObject["lz_start"];
                Info.info.lz_full = (double)jsonObject["lz_full"];

                file.Close();
            }
            
            //test
        }//负责存储【记录时间】【回满时间】【初始理智】【理智上限】

        public void Write() //写入文件
        {
            string output = JsonConvert.SerializeObject(Info.info, Formatting.Indented);    //返回缩进的 Json 字符串
            File.WriteAllText("info.json", output); //输出json内容到info.json

        }

        private void Window_Closed(object sender, EventArgs e)  //当窗口关闭时，自动保存
        {
            Write();
        }

        
        private void Mouse_MouseMove(object sender, MouseEventArgs e)   //无边框时拖动
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)   //关闭窗口按钮
        {
            this.Hide();
        }

        public void Set_StartUpLocation()   //设置窗口启动初始位置
        {
            var desktopWorkingArea = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea;
            this.Left = desktopWorkingArea.Right - this.Width - 5;
            this.Top = desktopWorkingArea.Bottom - this.Height - 65;
        }
    }
}