using LiveCharts;
using LiveCharts.Wpf;
using Logix;
using System;
using System.Net;
using System.Windows;
using System.Windows.Threading;

namespace LiveTrend
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer timer;
        private Random random = new Random();
        public SeriesCollection SeriesCollection { get; set; }
        LogixTcpClient plc;

        public MainWindow()
        {
            InitializeComponent();
            plc = SetupConnection("10.69.46.13", 3);

            SeriesCollection = new SeriesCollection {
                new LineSeries
                {
                    Title = "ID",
                    Values = new ChartValues<Int32> { }
                },
                new LineSeries
                {
                    Title = "RequestID",
                    Values = new ChartValues<Int32> { }
                },
                new LineSeries
                {
                    Title = "Preset",
                    Values = new ChartValues<Int32> { }
                },
                new LineSeries
                {
                    Title = "Accumulation",
                    Values = new ChartValues<Int32> { }
                }
            };

            DataContext = this;

            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            timer.Tick += TimerOnTick;
            timer.Start();
        }

        private void TimerOnTick(object sender, EventArgs e)
        {
            var a = plc.ReadInt32("DT_Lv2RequestID");
            var b = plc.ReadInt32("Lv2_ReqTimeData.RequestID");
            var c = plc.ReadInt32("DT_ReqTime.Timer.PRE");
            var d = plc.ReadInt32("DT_ReqTime.Timer.ACC");

            SeriesCollection[0].Values.Add(a);
            SeriesCollection[1].Values.Add(b);
            SeriesCollection[2].Values.Add(c);
            SeriesCollection[3].Values.Add(d);

            // Optionally, limit the number of data points for each series
            const int maxPoints = 20;
            foreach (var series in SeriesCollection)
            {
                if (series.Values.Count > maxPoints)
                    series.Values.RemoveAt(0);
            }
        }

        public LogixTcpClient SetupConnection(string ip, int controllerSlot)
        {
            IPAddress ipAddress = IPAddress.Parse(ip);
            IPEndPoint endpoint = new IPEndPoint(ipAddress, 44818);
            LogixTcpClient plc = new LogixTcpClient(endpoint, 1, controllerSlot);

            return plc;

        }
    }
}
