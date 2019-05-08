using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using Application = System.Windows.Application;
using MenuItem = System.Windows.Forms.MenuItem;
using MessageBox = System.Windows.MessageBox;
using TextBox = System.Windows.Controls.TextBox;


namespace GoogleTranslate.Desktop
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly RestClient _client = new RestClient("https://translate.google.cn");

        private readonly TranslateModel _translateModel = new TranslateModel();

        private readonly NotifyIcon _notifyIcon = new NotifyIcon();

        /// <summary>
        /// 当前窗口句柄
        /// </summary>
        private IntPtr m_Hwnd = new IntPtr();

        public MainWindow()
        {
            DataContext = _translateModel;
            Icon = BitmapFrame.Create(new Uri("pack://application:,,,/logo.ico", UriKind.RelativeOrAbsolute));
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            _notifyIcon.Text = @"Google translate desktop";
            _notifyIcon.Visible = true;
            _notifyIcon.Icon = new System.Drawing.Icon("logo.ico");
            _notifyIcon.DoubleClick += NotifyIcon_DoubleClick;



            Closing += MainWindow_Closing;

            var menuItems = new[]
            {
                //new MenuItem("热键",SetHotKey),
                new MenuItem("开机启动",SetAutoStartup) { Name="AutoStartup", Checked=AutoStartup.IsExistKey("Google translate desktop") , },
                new MenuItem("开打主窗口", Show),
                new MenuItem("退出", Exit)
            };
            _notifyIcon.ContextMenu = new System.Windows.Forms.ContextMenu(menuItems);
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            m_Hwnd = new WindowInteropHelper(this).Handle;
            var hWndSource = HwndSource.FromHwnd(m_Hwnd);
            // 添加处理程序
            if (hWndSource != null) hWndSource.AddHook(WndProc);

            HotKeyHelper.RegisterHotKey(new HotKeyModel
            {
                IsUsable = true,
                IsSelectCtrl = true,
                SelectKey = 49,
                Name = EHotKeySetting.打开主窗口.ToString()
            }, m_Hwnd);
        }

        private void SetAutoStartup(object sender, EventArgs e)
        {
            var menu = (MenuItem)sender;
            if (AutoStartup.SelfRunning(!menu.Checked, "Google translate desktop",
                System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "GoogleTranslate.Desktop.exe")))
            {
                menu.Checked = !menu.Checked;
            }
            else
                MessageBox.Show("设置失败，请以管理员身份运行！", "失败");

        }

        private void SetHotKey(object sender, EventArgs e)
        {
            new HotKeySettingWindow().ShowDialog();
        }

        private void Show(object sender, EventArgs e)
        {
            ShowWindow();
        }
        private void Exit(object sender, EventArgs e)
        {
            _notifyIcon.Visible = false;
            Application.Current.Shutdown();
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            HideWindow();
        }


        private void NotifyIcon_DoubleClick(object sender, EventArgs e)
        {
            ShowWindow();
        }

        private void HideWindow()
        {
            WindowState = WindowState.Minimized;
            ShowInTaskbar = false;
        }

        private void ShowWindow()
        {
            WindowState = WindowState.Normal;
            ShowInTaskbar = true;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //Keyboard.Focus(InputTextBox);
            //var textBox = (TextBox)sender;
            var r = InputTextBox.Focus();
            //MessageBox.Show(r.ToString());

        }

        private void Input_TextChanged(object sender, TextChangedEventArgs e)
        {
            var input = (TextBox)sender;
            Translate(input.Text);
        }

        private void OpenHelpClick(object sender, CanExecuteRoutedEventArgs e)
        {
            MessageBox.Show(this, "Alt+H:帮助\nAlt+S:切换目标语言", "快捷键");
        }


        private void SwitchLanguageClick(object sender, CanExecuteRoutedEventArgs e)
        {

        }

        private void Translate(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                _translateModel.TranslateResult = "";
                return;
            }

            Task.Run(async () =>
            {
                try
                {
                    var tick = tk(input, "432558.706957580");
                    var request = new RestRequest($"translate_a/single?client=webapp&ie=UTF-8&sl=auto&tl=en&hl=zh-CN&dt=at&dt=bd&dt=ex&dt=ld&dt=md&dt=qca&dt=rw&dt=rm&dt=ss&dt=t&dt=gt&pc=1&otf=1&ssel=0&tsel=0&kc=1&tk={tick}&q={HttpUtility.UrlEncode(input)}");
                    var response = await _client.ExecuteGetTaskAsync(request);
                    if (response.IsSuccessful)
                    {
                        var json = JsonConvert.DeserializeObject<JArray>(response.Content);
                        var res = new StringBuilder();
                        if (json?[0] != null)
                        {
                            foreach (var item in json?[0])
                            {
                                res.Append(item[0].Value<string>());
                            }
                            _translateModel.TranslateResult = res.ToString();
                        }
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }
            });
        }



        #region
        private string b(long a, string b)
        {
            for (int d = 0; d < b.Length - 2; d += 3)
            {
                char c = b.charAt(d + 2);
                int c0 = 'a' <= c ? c.charCodeAt(0) - 87 : ExtensionMethods.Number(c);
                long c1 = '+' == b.charAt(d + 1) ? a >> c0 : a << c0;
                a = '+' == b.charAt(d) ? a + c1 & 4294967295 : a ^ c1;
            }
            a = ExtensionMethods.Number(a);
            return a.ToString();
        }

        private string tk(string a, string TKK)
        {
            string[] e = TKK.Split('.');
            int d = 0;
            int h = 0;
            h = ExtensionMethods.Number(e[0]);
            byte[] g0 = Encoding.UTF8.GetBytes(a);
            long aa = h;
            for (d = 0; d < g0.Length; d++)
            {
                aa += g0[d];
                aa = Convert.ToInt64(b(aa, "+-a^+6"));
            }
            aa = Convert.ToInt64(b(aa, "+-3^+b+-f"));
            long bb = aa ^ ExtensionMethods.Number(e[1]);
            aa = bb;
            aa = aa + bb;
            bb = aa - bb;
            aa = aa - bb;
            if (0 > aa)
            {
                aa = (aa & 2147483647) + 2147483648;
            }
            aa %= (long)1e6;
            return aa + "." + (aa ^ h);
        }
        #endregion


        /// <summary>
        /// 窗体回调函数，接收所有窗体消息的事件处理函数
        /// </summary>
        /// <param name="hWnd">窗口句柄</param>
        /// <param name="msg">消息</param>
        /// <param name="wideParam">附加参数1</param>
        /// <param name="longParam">附加参数2</param>
        /// <param name="handled">是否处理</param>
        /// <returns>返回句柄</returns>
        private IntPtr WndProc(IntPtr hWnd, int msg, IntPtr wideParam, IntPtr longParam, ref bool handled)
        {
            switch (msg)
            {
                case HotKeyManager.WM_HOTKEY:
                    int sid = wideParam.ToInt32();
                    if (sid == HotKeyHelper.GetHotKeySetting(EHotKeySetting.打开主窗口))
                    {
                        if (ShowInTaskbar)
                            HideWindow();
                        else
                            ShowWindow();
                    }
                    handled = true;
                    break;
            }
            return IntPtr.Zero;
        }
    }
}
