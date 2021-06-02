using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
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

namespace m3u8Tomp4 {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        ViewModel vm = null;
        public MainWindow() {
            InitializeComponent();
            vm = new ViewModel();
            this.DataContext = vm;
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            //ffmepgは必須
            if (!File.Exists(vm.ffmpeg) || !vm.ffmpeg.EndsWith("ffmpeg.exe")) {
                MessageBox.Show("ffmpeg.exe Not Found");
                return;
            }
            //NAMEは必須
            if (string.IsNullOrEmpty(vm.NAME)) {
                MessageBox.Show("Invalid NAME");
                return;
            }
            //m3u8のURLを取得
            var url = Getm3u8Uri();
            if (!url.StartsWith("http")) {
                MessageBox.Show("Invalid URL");
                return;
            }
            //保存するパスの取得
            var path = System.IO.Path.Combine(vm.DIR, vm.NAME);
            if (File.Exists(path)) {
                MessageBox.Show("File Exists");
                return;
            }
            //ffmpegを使って変換
            var p = new Process();
            p.StartInfo = new ProcessStartInfo(vm.ffmpeg);
            p.StartInfo.Arguments = $"-i \"{url}\" -vcodec copy -acodec copy -absf aac_adtstoasc \"{path}\"";
            p.Start();
        }

        private string Getm3u8Uri() {
            if (vm.URL.EndsWith(".m3u8")) return vm.URL;

            var r = new Regex("(?<=\")[^\"]+?\\.m3u8(?=\")", RegexOptions.IgnoreCase);//""で囲まれてるm3u8を探す

            var wc = new WebClient();
            try {
                var html = wc.DownloadString(vm.URL);
                var m3u8 = r.Match(html).Value.Replace("\\","");//JavaScript内文字列の場合\が入っているかもしれないので削除
                if (m3u8.StartsWith("http")) return m3u8;
                return new Uri(new Uri(vm.URL), m3u8).AbsoluteUri;
            } catch (Exception ex) {
                return null;
            }
        }
    }
    public class ViewModel : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        string _URL = "";
        public string URL {
            get { return _URL; }
            set {
                _URL = value;
                OnPropertyChanged("URL");
            }
        }
        string _DIR = Properties.Settings.Default.OutputDirectory;
        public string DIR {
            get { return _DIR; }
            set {
                _DIR = value;
                OnPropertyChanged("DIR");
                Properties.Settings.Default.OutputDirectory = value;
                Properties.Settings.Default.Save();
            }
        }
        string _NAME = Properties.Settings.Default.FileName;
        public string NAME {
            get { return _NAME; }
            set {
                _NAME = value;
                OnPropertyChanged("NAME");
                Properties.Settings.Default.FileName = value;
                Properties.Settings.Default.Save();
            }
        }
        string _ffmpeg = Properties.Settings.Default.ffmpegPath;
        public string ffmpeg {
            get { return _ffmpeg; }
            set {
                _ffmpeg = value;
                OnPropertyChanged("ffmpeg");
                Properties.Settings.Default.ffmpegPath = value;
                Properties.Settings.Default.Save();
            }
        }
    }
}
