using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace MultidumperGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Dictionary<int, double> _progressDictionary;
        private bool _canceled;
        private MDOut _songInfo;
        private IncomingChannelInfo _channelInfo;
        private ObservableCollection<ChannelInfo> _channelInfoParsed;
        private System.Diagnostics.Process _infoProcess;
        private System.Diagnostics.Process _probeProcess;
        private System.Diagnostics.Process _dumpingProcess;
        private OpenFileDialog _ofDialog;

        public MainWindow()
        {

            InitializeComponent();

            _infoProcess = new Process()
            {
                EnableRaisingEvents = true
            };
            _probeProcess = new Process()
            {
                EnableRaisingEvents = true
            };
            _dumpingProcess = new Process()
            {
                EnableRaisingEvents = true
            };

            _infoProcess.OutputDataReceived += (sender, args) =>
            {
                if (args.Data != "" || args.Data != null)
                {
                    _songInfo = JsonConvert.DeserializeObject<MDOut>(args.Data);
                    _infoProcess.CancelOutputRead();

                    this.Dispatcher.Invoke(() =>
                    {
                        txtGame.Text = _songInfo.Containerinfo.Game;
                        txtSystem.Text = _songInfo.Containerinfo.System;
                        txtDumper.Text = _songInfo.Containerinfo.Dumper;
                        txtCopyright.Text = _songInfo.Containerinfo.Copyright;

                        thrInfo.Visibility = Visibility.Hidden;
                        thrProbe.Visibility = Visibility.Visible;

                        _channelInfoParsed = new ObservableCollection<ChannelInfo>();

                        Probe();


                        var indexed = new List<IndexedSong>();

                        for (int i = 0; i < _songInfo.Songs.Count; i++)
                        {
                            var s = _songInfo.Songs[i];
                            s.Name = s.Name == "" ? null : s.Name;
                            indexed.Add(new IndexedSong(i, s));
                        }

                        lstSubSongs.ItemsSource = indexed;

                        
                    });
                }
            };

            _infoProcess.Exited += (sender, args) => { };
            double mxp = 0.0;

            _probeProcess.OutputDataReceived += (sender, args) =>
            {
                string line = args.Data;

                if (line == null)
                {
                    _probeProcess.CancelOutputRead();
                    return;
                }
                if (line != "")
                {
                    if (line[0] == 'p')
                    {
                        var x = line.Split('|');
                        mxp = Double.Parse(x[2]);
                        this.Dispatcher.Invoke(() =>
                        {
                            thrProbe.Maximum = Double.Parse(x[2]);
                            thrProbe.Value = Double.Parse(x[1]);
                        });
                    }
                    else
                    {
                        _channelInfo = JsonConvert.DeserializeObject<IncomingChannelInfo>(line);
                        foreach (var channel in _channelInfo.Channels)
                        {
                            _channelInfoParsed.Add(new ChannelInfo()
                            {
                                ChannelName = channel,
                                CurrentProgress = 0.0,
                                MaximumProgress = mxp
                            });
                        }

                        this.Dispatcher.Invoke(() =>
                        {
                            lstInfo.Visibility = Visibility.Visible;
                            lstInfo.ItemsSource = _channelInfoParsed;

                        });


                    }
                }


            };

            _probeProcess.Exited += (sender, args) =>
            {
             
                
            };

            string lineD;

            _dumpingProcess.OutputDataReceived += (sender, args) =>
            {
                lineD = args.Data;

                if (lineD == null)
                {
                    _dumpingProcess.CancelOutputRead();
                    return;
                }

                if (lineD != "")
                {
                    var x = lineD.Split('|');
                    _channelInfoParsed[Int32.Parse(x[0])].MaximumProgress = Double.Parse(x[2]);
                    _channelInfoParsed[Int32.Parse(x[0])].CurrentProgress = Double.Parse(x[1]);

                }

            };

            _dumpingProcess.Exited += (sender, args) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    btnLoad.IsEnabled = true;
                    btnDump.IsEnabled = true;


                });
                
                Process.Start("explorer", $"/select,\"{_ofDialog.FileName}\"");

            };
        }

        private void Probe()
        {
            _probeProcess.StartInfo = new ProcessStartInfo()
            {
                FileName = @"multidumper.exe",
                Arguments = $"\"{_ofDialog.FileName}\" --probe",
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };
            thrProbe.Visibility = Visibility.Visible;
            lstInfo.Visibility = Visibility.Hidden;
            _probeProcess.Start();
            _probeProcess.BeginOutputReadLine();
        }

        private void txtFilename_TextChanged(object sender, TextChangedEventArgs e)
        {
        }

        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            _ofDialog = new OpenFileDialog()
            {
                Filter = "Supported formats|*.ay;*.gbs;*.gym;*.hes;*.kss;*.nsf;*.nsfe;*.sap;*.sfm;*.sgc;*.s" +
                         "pc;*.spu;*.vgm;*.vgz"
            };

            if (_ofDialog.ShowDialog() != true) return;
            txtFilename.Text = _ofDialog.FileName;

            thrInfo.Visibility = Visibility.Visible;

            LoadFile();
        }

        private void LoadFile()
        {
            _infoProcess.StartInfo = new ProcessStartInfo()
            {
                FileName = @"multidumper.exe",
                Arguments = $"\"{_ofDialog.FileName}\" --json",
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };
            btnDump.IsEnabled = false;

            _infoProcess.Start();
            _infoProcess.BeginOutputReadLine();
        }

        private void lstSubSongs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            btnDump.IsEnabled = true;
        }

        private void btnDump_Click(object sender, RoutedEventArgs e)
        {
            btnLoad.IsEnabled = false;
            btnDump.IsEnabled = false;
            _dumpingProcess.StartInfo = new ProcessStartInfo()
            {
                FileName = @"multidumper.exe",
                Arguments = _ofDialog.FileName.EndsWith(".spc",StringComparison.InvariantCultureIgnoreCase) ||
                _ofDialog.FileName.EndsWith(".sfm", StringComparison.InvariantCultureIgnoreCase)
                ? $"\"{_ofDialog.FileName}\" {lstSubSongs.SelectedIndex} {_channelInfoParsed.Count}" :
                    $"\"{_ofDialog.FileName}\" {lstSubSongs.SelectedIndex}",
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true,

            };
            _dumpingProcess.Start();
            _dumpingProcess.BeginOutputReadLine();

        }
    }
}