using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Application = System.Windows.Application;
using Label = System.Windows.Controls.Label;
using MenuItem = System.Windows.Forms.MenuItem;
using TextBox = System.Windows.Controls.TextBox;


namespace GoogleTranslate.Desktop
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private readonly TranslateModel _translateModel = new TranslateModel();

        private readonly NotifyIcon _notifyIcon = new NotifyIcon();

        /// <summary>
        /// 当前窗口句柄
        /// </summary>
        private IntPtr m_Hwnd = new IntPtr();

        public MainWindow()
        {
            DataContext = _translateModel;
            InitializeComponent();
            _notifyIcon.Text = @"Google translate desktop";
            _notifyIcon.Visible = true;
            _notifyIcon.Icon = new System.Drawing.Icon(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logo.ico"));
            _notifyIcon.DoubleClick += NotifyIcon_DoubleClick;
            if (!AppSettingsManager.ExistConfig())
            {
                _notifyIcon.BalloonTipTitle = @"Hi, 我在这儿呢";
                _notifyIcon.BalloonTipText = @"使用快捷键 Control+` 可以打开我哦";
                _notifyIcon.ShowBalloonTip(5000);
            }

            Closing += MainWindow_Closing;

            var menuItems = new[]
            {
                //new MenuItem("热键",SetHotKey),
                new MenuItem("开机启动",SetAutoStartup) { Name="AutoStartup", Checked=AutoStartup.IsExistKey("Google translate desktop") , },
                new MenuItem("开打主窗口( Control+` )", Show),
                new MenuItem("退出", Exit)
            };
            _notifyIcon.ContextMenu = new System.Windows.Forms.ContextMenu(menuItems);

            _translateModel.OnChangeTargetLanguage += async arg => { await _translateModel.Translate(); };
            var appSettings = AppSettingsManager.Read();
            if (appSettings.TopMost)
            {
                Topmost = true;
                TopMostIcon.Source = BitmapFrame.Create(new Uri("pack://application:,,,/resources/topmost_yes.ico", UriKind.RelativeOrAbsolute));
                TopMostIcon.ToolTip = "取消置顶";
            }
            else
            {
                Topmost = false;
                TopMostIcon.Source = BitmapFrame.Create(new Uri("pack://application:,,,/resources/topmost_no.ico", UriKind.RelativeOrAbsolute));
                TopMostIcon.ToolTip = "置顶";
            }

            var lag = _translateModel.Languages.FirstOrDefault(p => p.Code == (string.IsNullOrWhiteSpace(appSettings.CurrentTargetLanguage) ? "en" : appSettings.CurrentTargetLanguage));
            if (lag != null)
            {
                _translateModel.TargetLanguage = lag.Code;
                _translateModel.TargetLanguageText = lag.Name;
            }



            InputTextBox.Focus();
            HideWindow();
        }

        private void TopMostIcon_MouseUp(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var appSettings = AppSettingsManager.Read();
            if (!appSettings.TopMost)
            {
                Topmost = true;
                TopMostIcon.Source = BitmapFrame.Create(new Uri("pack://application:,,,/resources/topmost_yes.ico", UriKind.RelativeOrAbsolute));
                TopMostIcon.ToolTip = "取消置顶";
            }
            else
            {
                Topmost = false;
                TopMostIcon.Source = BitmapFrame.Create(new Uri("pack://application:,,,/resources/topmost_no.ico", UriKind.RelativeOrAbsolute));
                TopMostIcon.ToolTip = "置顶";
            }

            appSettings.TopMost = !appSettings.TopMost;
            AppSettingsManager.UpdateAppSettings();
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
                SelectKey = 192,
                Name = EHotKeySetting.ShowMainWindow.ToString()
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
                this.ShowMessageAsync("错误", "设置失败，请以管理员身份运行！");

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
            _translateModel.Dispose();
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
            Visibility = Visibility.Hidden;
            ShowInTaskbar = false;
        }

        private void ShowWindow()
        {
            //WindowState = WindowState.Normal;
            Visibility = Visibility.Visible;
            ShowInTaskbar = true;
            InputTextBox.Focus();
            InputTextBox.SelectAll();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            InputTextBox.Focus();
        }

        private void Input_TextChanged(object sender, TextChangedEventArgs e)
        {
            var input = (TextBox)sender;
            _translateModel.InputText = input.Text;
        }

        private void OpenHelpClick(object sender, CanExecuteRoutedEventArgs e)
        {
            OpenHelp();
        }

        private void HelpIcon_MouseUp(object sender, MouseButtonEventArgs e)
        {
            OpenHelp();
        }

        private void OpenHelp()
        {
            this.ShowMessageAsync("帮助", "Alt+H : 打开帮助\n\nControl+` : 主窗口显隐切换\n\nAlt+C : 复制翻译结果\n\nAlt+M : 查看更多翻译结果");
        }


        private void SwitchLanguageClick(object sender, CanExecuteRoutedEventArgs e)
        {
            //SwitchLanguageDropDown.RaiseEvent(new RoutedEventArgs(DropDownButton.MouseEnterEvent));
        }






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
                    if (sid == HotKeyHelper.GetHotKeySetting(EHotKeySetting.ShowMainWindow))
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

        private async void SelectRecentlyUsedLanguage(object sender, MouseButtonEventArgs e)
        {
            var label = (System.Windows.Controls.Label)sender;
            var code = label.Uid;
            var lag = _translateModel.Languages.FirstOrDefault(p => p.Code == code);
            if (lag == null)
                return;
            _translateModel.TargetLanguage = code;
            _translateModel.TargetLanguageText = lag.Name;
            await _translateModel.Translate();
        }

        private void CleanInputClick(object sender, CanExecuteRoutedEventArgs e)
        {
            InputTextBox.Text = "";
        }

        private void GithubIcon_MouseUp(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/1100100/GoogleTranslate.Desktop");
        }



        private void ShowMore_Click(object sender, MouseButtonEventArgs e)
        {
            OpenOrCloseFlyout();
        }

        private void SelectMoreItem_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var label = (Label)sender;
            InputTextBox.Text = label.Content.ToString();
            OpenOrCloseFlyout();
        }

        private void OpenOrCloseFlyout()
        {
            var flyout = this.Flyouts.Items[0] as Flyout;
            if (flyout == null)
            {
                return;
            }
            flyout.IsOpen = !flyout.IsOpen;
        }

        private void ShowMoreClick(object sender, CanExecuteRoutedEventArgs e)
        {
            if (_translateModel.MoreInformation != null && _translateModel.MoreInformation.Any())
                OpenOrCloseFlyout();
        }

        private void CopyClick(object sender, CanExecuteRoutedEventArgs e)
        {
            System.Windows.Clipboard.SetText(_translateModel.TranslateResult);
        }
    }
}
