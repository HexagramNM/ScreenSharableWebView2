using System.Diagnostics;
using System.Management;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

using NAudio.Wave;
using NAudio.CoreAudioApi;

namespace ScreenSharableWebView2
{
    /// <summary
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        // WPFアプリのプロセスID
        int currentProcessId = Process.GetCurrentProcess().Id;

        // WebView2プロセスの名前
        const string webviewProcessName = "msedgewebview2";

        // WebView2プロセスからの音声をキャプチャするためのオブジェクト (NAudio)
        WasapiCapture? webviewAudioCapture = null;

        // WPFアプリから流す音声データを格納するバッファオブジェクト (NAudio)
        BufferedWaveProvider? waveProvider = null;

        // WPFアプリからwaveProviderにある音声を再生するためのオブジェクト (NAudio)
        WasapiOut? waveOut = null;

        // AudioRedirectを再度実行するためのタイマー
        System.Timers.Timer? redirectAudioTimer = null;

        public MainWindow()
        {
            InitializeComponent();

            AudioRedirect();

            //ブラウザウィンドウの配置設定
            int windowBarHeight = 5;

            MainWindow1.Width = App.width;
            MainWindow1.Height = App.height + windowBarHeight * 2;

            MainGrid.Width = App.width;
            MainGrid.Height = App.height + windowBarHeight * 2;
            MainGrid.Margin = new Thickness(0, 0, 0, 0);

            MainWebBrowser.Source = App.path;

            // ESCキーでウィンドウを閉じるイベントを設定します。
            MainWindow1.KeyDown += (s, e) =>
            {
                if (e.Key == Key.Escape)
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        CloseWindow();
                    }), DispatcherPriority.ApplicationIdle);
                }
            };

            // マウスドラッグでウィンドウを移動できるようイベントを設定します。
            // 上端をクリックすることでドラッグできるようにしています。
            MainGrid.MouseLeftButtonDown += (s, e) =>
            {
                if (e.ButtonState == MouseButtonState.Pressed)
                {
                    MainWindow1.DragMove();
                }
            };
        }

        // WebView2プロセスからの音声データを取得した際に呼び出されるイベントで、
        // 取得した音声データをそのままWPFで再生する音声データ (waveProvider) に追加しています。
        private void ReceivedCaptureAudio(object? sender, WaveInEventArgs e)
        {
            waveProvider?.AddSamples(e.Buffer, 0, e.BytesRecorded);
        }

        // 引数で指定したIDのプロセスがWPFアプリのプロセスの子孫にあたるかをチェックし、
        // 子孫にあたるならtrueを、そうでなければfalseを返します。
        // 親プロセスのIDを取得する方法は以下のページを参考にしています。
        // 
        // 【C#】親プロセスのプロセスIDを取得する（祖父プロセスIDも）
        // https://umateku.com/archives/3300
        private bool IsDescendantsProcess(int processId)
        {
            int checkingProcessId = processId;
            while (true)
            {
                string query = $"SELECT ParentProcessId FROM Win32_Process WHERE ProcessId = {checkingProcessId}";
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", query);
                bool foundParent = false;
                foreach (ManagementObject obj in searcher.Get())
                {
                    int parentProcessId = Convert.ToInt32(obj["ParentProcessId"]);
                    // 最終的に親プロセスIDが0になるので、その場合にチェックを終了します。
                    // （そうしないと無限ループになってしまいます。）
                    if (parentProcessId <= 0)
                    {
                        break;
                    }

                    foundParent = true;
                    if (currentProcessId == parentProcessId)
                    {
                        return true;
                    }
                    else
                    {
                        checkingProcessId = parentProcessId;
                    }
                    break;
                }

                if (!foundParent)
                {
                    break;
                }
            }

            return false;
        }

        // 今回のWebView2プロセスの音声をキャプチャし、WPFアプリで再生できるよう設定するための関数です。
        // MainWindowクラスのコンストラクタで呼び出します。
        private async void AudioRedirect()
        {
            redirectAudioTimer?.Dispose();
            redirectAudioTimer = null;

            // この部分でWebView2で音声を流すプロセスを探しています。
            // スピーカーなど音声を再生するデバイスを1つずつ見ていき、そのデバイスに音声を渡しているプロセスの中からWebView2プロセスを探しています。
            MMDeviceEnumerator enumerator = new MMDeviceEnumerator();
            MMDeviceCollection devices = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
            foreach (MMDevice device in devices)
            {
                SessionCollection sessions = device.AudioSessionManager.Sessions;
                for (int i = 0; i < sessions.Count; i++)
                {
                    AudioSessionControl session = sessions[i];
                    try
                    {
                        Process process = Process.GetProcessById((int)session.GetProcessID);

                        if (IsDescendantsProcess((int)session.GetProcessID)
                            && process.ProcessName.IndexOf(webviewProcessName, StringComparison.OrdinalIgnoreCase) != -1)
                        {
                            // WebView2で音声を流すプロセスが見つかったら、WebView2プロセスから音声をキャプチャする設定や、
                            // キャプチャした音声をWPFで再生する設定を行います。
                            MMDevice speakerDevice = new MMDeviceEnumerator().GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);

                            webviewAudioCapture = await WasapiCapture.CreateForProcessCaptureAsync((int)session.GetProcessID, true);
                            waveProvider = new BufferedWaveProvider(webviewAudioCapture.WaveFormat);
                            waveOut = new WasapiOut(speakerDevice, AudioClientShareMode.Shared, true, 50);

                            waveProvider.DiscardOnBufferOverflow = true;
                            waveProvider.BufferDuration = TimeSpan.FromMilliseconds(100.0);

                            waveOut.Init(waveProvider);
                            waveOut.Play();

                            webviewAudioCapture.DataAvailable += ReceivedCaptureAudio;
                            webviewAudioCapture.StartRecording();
                            return;
                        }
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
            }

            // WebView2で音声を流すプロセスが見つからなかった場合は、タイマーを設定し、
            // 1秒後再度AudioRedirectの処理が行われるようにしておきます。
            redirectAudioTimer = new System.Timers.Timer(1000);
            redirectAudioTimer.Elapsed += (s, e) => { AudioRedirect(); };
            redirectAudioTimer.AutoReset = false;
            redirectAudioTimer.Enabled = true;
            redirectAudioTimer.Start();
        }

        // ウィンドウの背景色をWebページの背景色に変更します。
        private async void MainWebBrowser_NavigationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            string bgColorCode = await MainWebBrowser.CoreWebView2.ExecuteScriptAsync("document.body.bgColor;");
            MainWindow1.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(bgColorCode.Replace("\"", "")));
        }

        // ESCキーが押された場合などに呼び出す、アプリケーションを終了させるための処理で、
        // webviewAudioCaptureやwaveOutを停止しておきます。
        private void CloseWindow()
        {
            webviewAudioCapture?.StopRecording();
            waveOut?.Stop();
            waveOut?.Dispose();

            Close();
        }
    }
}