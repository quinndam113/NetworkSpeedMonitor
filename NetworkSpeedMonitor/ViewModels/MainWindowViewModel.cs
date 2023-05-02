using Avalonia.Threading;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;

namespace NetworkSpeedMonitor.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private NetworkInterface _currentNetworkType;
        public NetworkInterface CurrentNetworkType
        {
            get { return _currentNetworkType; }
            set
            {
                this.RaiseAndSetIfChanged(ref _currentNetworkType, value);
            }
        }

        public List<NetworkInterface> Networks { get; set; }

        private string _downloadSpeed = "↓";
        public string DownloadSpeed
        {
            get { return _downloadSpeed; }
            set
            {
                this.RaiseAndSetIfChanged(ref this._downloadSpeed, value);
            }
        }

        private string _uploadSpeed = "↑";
        public string UploadSpeed
        {
            get { return _uploadSpeed; }
            set
            {
                this.RaiseAndSetIfChanged(ref this._uploadSpeed, value);
            }
        }

        private long _totalReceive = 0;
        private long _totalSend = 0;
        private DispatcherTimer _timer;
        public MainWindowViewModel()
        {
            Networks = NetworkInterface.GetAllNetworkInterfaces()
                .Where(x => x.OperationalStatus == OperationalStatus.Up)
                .ToList();

            CurrentNetworkType = Networks.FirstOrDefault();

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += _timer_Tick;
            _timer.Start();
        }

        private void _timer_Tick(object? sender, EventArgs e)
        {
            if (CurrentNetworkType != null)
            {
                var networkStat = CurrentNetworkType.GetIPv4Statistics();
                if (networkStat != null)
                {
                    var upSpeed =  networkStat.BytesSent - _totalSend;
                    var downSpeed = networkStat.BytesReceived - _totalReceive;

                    UploadSpeed = "↑ " + FormatSpeedKbs(upSpeed);
                    DownloadSpeed = "↓ " +  FormatSpeedKbs(downSpeed); 

                    _totalReceive = networkStat.BytesReceived;
                    _totalSend = networkStat.BytesSent;
                }
            }
        }

        private string FormatSpeedKbs(long number)
        {
            if (number < 0)
            {
                return "-";
            }

            double result = number;
            string unit = "bytes/s";

            if (result > 1000000)
            {
                result = result / 1024 / 1024;
                unit = "MB/s";
            }
            else if (result > 1000)
            {
                result = (double)number / 1024;
                unit = "KB/s";
            }

            return string.Format("{0:n0} " + unit, result);
        }
    }
}