using System.Collections;
using System.Collections.ObjectModel;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using HttpRequest.IServices;
using HttpRequest.Services;
using HttpRequestClient.Services;
using MyTestMAUI.IService;

namespace MyTestMAUI
{
    public partial class MainPage : ContentPage
    {
        private readonly IToastService _toastService;

        public ObservableCollection<string> PingResults { get; set; }

        private Dictionary<string, HttpRestClient> HttpRestClientDir { get; set; } = [];

        public MainPage(IToastService toastService)
        {
            InitializeComponent();
            PingResults = new ObservableCollection<string>();
            ResultsListView.ItemsSource = PingResults;

            _toastService = toastService;


            // 初始化 Stepper 的值显示
            StepperValueLabel.Text = NumberStepper.Value.ToString();
        }
        private void OnStepperValueChanged(object sender, ValueChangedEventArgs e)
        {
            // 更新标签显示的 Stepper 值
            StepperValueLabel.Text = e.NewValue.ToString();
        }
        private async void OnScanButtonClicked(object sender, EventArgs e)
        {
            PingResults.Clear();
            var localIPs = GetLocalIPAddress();
            if (localIPs == null || localIPs.Count == 0)
            {
                await DisplayAlert("Error", "Unable to get local IP address", "OK");
                return;
            }
            foreach (var ip in localIPs) 
            {
                if (ip.StartsWith("127.0.0"))
                {
                    continue;
                }
                await ScanNetworkAsync(ip);
            }

        }

        private async void OnShutdownClicked(object sender, EventArgs e)
        {
            await RemoteShutdown(0);
        }

        private async Task RemoteShutdown(int seconds)
        {
            var ip = ResultsListView.SelectedItem?.ToString();
            if(ip == null || ip.Contains("不支持")) 
            {
                ShowToast("请选择一个受支持的主机地址");
                return; 
            }


            HttpRestClient httpRestClient = HttpRestClientDir.ContainsKey(ip) ? HttpRestClientDir[ip] : new HttpRestClient("http://" + ip + ":5555/");
            HttpRestClientDir.TryAdd(ip, httpRestClient);

            IRemoteShutdownRequestService remoteShutdownRequestService = new RemoteShutdownRequestService(httpRestClient);

            var testHttpResult = await remoteShutdownRequestService.SetDelayShutdown(seconds);
            //Console.WriteLine(response.Content);
            string result;
            if (testHttpResult.Status)
            {
                result = seconds == 0 ? $"主机:{ip} 即将关闭" : $"主机:{ip} 即将在 {seconds} 秒后关闭";
            }
            else
            {
                result = $"发生错误: {testHttpResult.Result}";
            }
            ShowToast(result);
        }

        private void ShowToast(string message)
        {
            _toastService.ShowToast(message);
        }

        private async void OnShutdownAfterClicked(object sender, EventArgs e)
        {
            await RemoteShutdown(60 * (int)NumberStepper.Value);
        }

        private List<string> GetLocalIPAddress()
        {
            var result = new List<string>();
            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus == OperationalStatus.Up)
                {
                    var ipProps = ni.GetIPProperties();
                    foreach (var ua in ipProps.UnicastAddresses)
                    {
                        if (ua.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            result.Add(ua.Address.ToString());
                        }
                    }
                }
            }
            return result;
        }

        private async Task ScanNetworkAsync(string localIP)
        {
            string[] ipParts = localIP.Split('.');
            string baseIP = string.Join(".", ipParts[0], ipParts[1], ipParts[2]) + ".";

            var tasks = new List<Task>();
            for (int i = 1; i < 255; i++)
            {
                string ip = baseIP + i;
                tasks.Add(Task.Run(async () =>
                {
                    using (Ping ping = new Ping())
                    {
                        try
                        {
                            PingReply reply = await ping.SendPingAsync(ip, 100);
                            if (reply.Status == IPStatus.Success)
                            {
                                HttpRestClient httpRestClient = HttpRestClientDir.ContainsKey(ip) ? HttpRestClientDir[ip] : new HttpRestClient("http://" + ip + ":5555/");
                                HttpRestClientDir.TryAdd(ip, httpRestClient);

                                ITestRequestService testRequestService = new TestRequestService(httpRestClient);

                                var testHttpResult = await testRequestService.TestHostConnection();
                                //Console.WriteLine(response.Content);
                                string result;
                                if (testHttpResult.Status)
                                {
                                    result = ip;
                                }
                                else
                                {
                                    result = "主机:" + ip + " 不支持";
                                }
                                AddViewListItem(PingResults, result);
                            }
                        }
                        catch (Exception ex)
                        {

                            /*MainThread.BeginInvokeOnMainThread(() =>
                            {
                                PingResults.Add($"Ping failed: {ip} - {ex.Message}");

                            });*/                        
                        }
                    }
                }));
            }
            await Task.WhenAll(tasks);
        }

        private void AddViewListItem<T,K>(T target,K values) where T : IList
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                target.Add(values);
            });
        }

        private async void OnShutdownCancelClicked(object sender, EventArgs e)
        {
            var ip = ResultsListView.SelectedItem?.ToString();
            if (ip == null || ip.Contains("不支持"))
            {
                ShowToast("请选择一个受支持的主机地址");
                return;
            }


            HttpRestClient httpRestClient = HttpRestClientDir.ContainsKey(ip) ? HttpRestClientDir[ip] : new HttpRestClient("http://" + ip + ":5555/");
            HttpRestClientDir.TryAdd(ip, httpRestClient);

            IRemoteShutdownRequestService remoteShutdownRequestService = new RemoteShutdownRequestService(httpRestClient);

            var testHttpResult = await remoteShutdownRequestService.CancelShutdown();
            //Console.WriteLine(response.Content);
            string result;
            if (testHttpResult.Status)
            {
                result = "已发送取消关机请求";
            }
            else
            {
                result = $"发生错误: {testHttpResult.Result}";
            }
            ShowToast(result);
        }
    }

}
